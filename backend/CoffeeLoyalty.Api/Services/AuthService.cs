using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Constants;
using CoffeeLoyalty.Api.Data;
using CoffeeLoyalty.Api.Dtos.Auth;
using CoffeeLoyalty.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// All authentication rules live here; the controller only maps HTTP to these calls.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly IFirebaseAuthService _firebase;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Creates the service.
    /// </summary>
    /// <param name="db">The application database context.</param>
    /// <param name="jwt">Issues our own tokens.</param>
    /// <param name="firebase">Verifies Firebase ID tokens.</param>
    /// <param name="logger">Diagnostics; never surfaced to the caller.</param>
    public AuthService(
        AppDbContext db,
        IJwtTokenService jwt,
        IFirebaseAuthService firebase,
        ILogger<AuthService> logger)
    {
        _db = db;
        _jwt = jwt;
        _firebase = firebase;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EmployeeLoginResponse> LoginEmployeeAsync(
        EmployeeLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var employee = await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Username == request.Username, cancellationToken);

        // An unknown username and a wrong password are deliberately indistinguishable:
        // a different response would let anyone enumerate valid staff usernames.
        if (employee is null || !VerifyPassword(request.Password, employee.PasswordHash))
        {
            throw ApiException.InvalidCredentials();
        }

        // Checked only after the password is proven correct, so the deactivated state
        // is disclosed to the account's real owner and nobody else (decision 18).
        if (!employee.IsActive)
        {
            throw ApiException.AccountDisabled();
        }

        var role = RoleNames.ToWireString(employee.Role);
        var token = _jwt.CreateToken(employee.Id, role, employee.FullName, out var expiresAt);

        return new EmployeeLoginResponse
        {
            Token = token,
            FullName = employee.FullName,
            Role = role,
            ExpiresAt = expiresAt
        };
    }

    /// <inheritdoc />
    public async Task<CustomerLoginResponse> LoginCustomerAsync(
        FirebaseLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var identity = await _firebase.VerifyIdTokenAsync(request.FirebaseIdToken, cancellationToken);

        // Firebase spells the number as the user typed it during OTP; the customer's
        // identity in our database is always the E.164 form (decision 11).
        var phoneNumber = JordanPhoneNumber.NormalizeOrThrow(identity.PhoneNumber);

        var customer = await FindOrCreateCustomerAsync(identity, phoneNumber, request.FullName, cancellationToken);

        var token = _jwt.CreateToken(customer.Id, RoleNames.Customer, customer.FullName, out var expiresAt);

        return new CustomerLoginResponse
        {
            Token = token,
            FullName = customer.FullName,
            PointsBalance = customer.PointsBalance,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>
    /// Resolves the customer behind a verified Firebase identity, creating the row on
    /// first login and linking the Firebase uid to an existing cashier-registered customer.
    /// </summary>
    /// <param name="identity">The verified Firebase identity.</param>
    /// <param name="phoneNumber">The normalized E.164 phone number.</param>
    /// <param name="fullName">Display name from the request; used only when creating.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The persisted customer.</returns>
    private async Task<Customer> FindOrCreateCustomerAsync(
        FirebaseIdentity identity,
        string phoneNumber,
        string? fullName,
        CancellationToken cancellationToken)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, cancellationToken);

        if (customer is not null)
        {
            LinkFirebaseUid(customer, identity.Uid);
            await _db.SaveChangesAsync(cancellationToken);
            return customer;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            // First login creates the account, and Customer.FullName is NOT NULL —
            // the app must collect a name before exchanging the token.
            throw ApiException.ValidationError("الاسم مطلوب عند تسجيل الدخول لأول مرة");
        }

        customer = new Customer
        {
            FullName = fullName.Trim(),
            PhoneNumber = phoneNumber,
            FirebaseUid = identity.Uid,
            PointsBalance = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Customers.Add(customer);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Two first-logins racing on the same phone number: the unique index rejects
            // the loser, which then reads the row the winner just committed.
            _logger.LogWarning(ex, "Concurrent customer creation for a phone number; re-reading the winning row.");

            _db.Entry(customer).State = EntityState.Detached;

            var winner = await _db.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, cancellationToken);

            if (winner is null)
            {
                // Not the race we assumed — some other write failure. Let it become a 500.
                throw;
            }

            customer = winner;
            LinkFirebaseUid(customer, identity.Uid);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return customer;
    }

    /// <summary>
    /// Attaches the Firebase uid on first link only. An already-linked customer keeps
    /// the uid they have — re-pointing an identity is an account takeover path, not a login concern.
    /// </summary>
    /// <param name="customer">The customer being logged in.</param>
    /// <param name="uid">The uid from the verified token.</param>
    private void LinkFirebaseUid(Customer customer, string uid)
    {
        if (customer.FirebaseUid is null)
        {
            customer.FirebaseUid = uid;
            return;
        }

        if (!string.Equals(customer.FirebaseUid, uid, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Customer {CustomerId} logged in with Firebase uid {IncomingUid} but is linked to a different uid; keeping the existing link.",
                customer.Id,
                uid);
        }
    }

    /// <summary>
    /// Verifies a password against a stored BCrypt hash, treating a corrupt or
    /// legacy hash as a failed login rather than a server error.
    /// </summary>
    /// <param name="password">The submitted plaintext password.</param>
    /// <param name="passwordHash">The stored BCrypt hash.</param>
    /// <returns><c>true</c> when the password matches.</returns>
    private bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException ex)
        {
            _logger.LogError(ex, "An employee row has an unreadable password hash.");
            return false;
        }
    }
}
