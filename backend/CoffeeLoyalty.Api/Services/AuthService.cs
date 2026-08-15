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
    /// <summary>
    /// A BCrypt hash of a random secret that was generated once and thrown away, at the
    /// same work factor the seeder hashes with. Verifying against it costs exactly what
    /// verifying a real employee's hash costs, which is the whole point — see
    /// <see cref="LoginEmployeeAsync"/>. No password matches it.
    /// </summary>
    private const string DummyPasswordHash = "$2a$11$Kit2aWZhM3sIlFL.s4AwaucIVyyrAR2IMJcIKae10tXHvYv9zWf9e";

    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly IFirebaseAuthService _firebase;
    private readonly IPasswordVerifier _passwords;
    private readonly ILoginThrottle _throttle;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Creates the service.
    /// </summary>
    /// <param name="db">The application database context.</param>
    /// <param name="jwt">Issues our own tokens.</param>
    /// <param name="firebase">Verifies Firebase ID tokens.</param>
    /// <param name="passwords">Verifies submitted passwords against stored hashes.</param>
    /// <param name="throttle">Blocks an account after repeated failures (decision 27).</param>
    /// <param name="logger">Diagnostics; never surfaced to the caller.</param>
    public AuthService(
        AppDbContext db,
        IJwtTokenService jwt,
        IFirebaseAuthService firebase,
        IPasswordVerifier passwords,
        ILoginThrottle throttle,
        ILogger<AuthService> logger)
    {
        _db = db;
        _jwt = jwt;
        _firebase = firebase;
        _passwords = passwords;
        _throttle = throttle;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EmployeeLoginResponse> LoginEmployeeAsync(
        EmployeeLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // Keyed on what the caller submitted, not on a row we found — the throttle must
        // behave identically for a real username and an invented one (decision 27).
        var throttleKey = EmployeeThrottleKey(request.Username);

        if (_throttle.IsBlocked(throttleKey))
        {
            throw ApiException.TooManyRequests();
        }

        var employee = await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Username == request.Username, cancellationToken);

        // An unknown username and a wrong password must be indistinguishable in the
        // response *and* in how long it takes to produce. Returning early on a missing
        // row would skip BCrypt (~100 ms) and make unknown usernames measurably faster
        // to reject, which is a username-enumeration oracle — so the submitted password
        // is run against a throwaway hash instead and both paths do the same work.
        var passwordMatches = _passwords.Verify(request.Password, employee?.PasswordHash ?? DummyPasswordHash);

        if (employee is null || !passwordMatches)
        {
            // Counted for the unknown username too: a username that never gets throttled
            // is a username an attacker knows does not exist.
            _throttle.RegisterFailure(throttleKey);
            throw ApiException.InvalidCredentials();
        }

        // Checked only after the password is proven correct, so the deactivated state
        // is disclosed to the account's real owner and nobody else (decision 18).
        // Not counted as a failure — the password was right; the account is simply closed.
        if (!employee.IsActive)
        {
            throw ApiException.AccountDisabled();
        }

        // Proven owner: earlier typos must not accumulate toward a block.
        _throttle.Reset(throttleKey);

        var role = RoleNames.ToWireString(employee.Role);
        var token = _jwt.CreateToken(employee.Id, role, employee.FullName, employee.TokenVersion, out var expiresAt);

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
        // A token that fails verification carries no identifier to count against, so the
        // throttle starts here. That is not a gap worth closing: a Firebase ID token is
        // signed by Google, so it is forged rather than guessed, and the attempts an
        // attacker can afford against it are bounded by nothing we could count anyway.
        var identity = await _firebase.VerifyIdTokenAsync(request.FirebaseIdToken, cancellationToken);

        // Firebase spells the number as the user typed it during OTP; the customer's
        // identity in our database is always the E.164 form (decision 11).
        var normalized = JordanPhoneNumber.TryNormalize(identity.PhoneNumber, out var phoneNumber) ? phoneNumber : null;

        // The phone number when the token carries a usable one, the uid otherwise — so a
        // token with an unusable phone claim is still counted against something stable.
        var throttleKey = CustomerThrottleKey(identity.Uid, normalized);

        if (_throttle.IsBlocked(throttleKey))
        {
            throw ApiException.TooManyRequests();
        }

        if (normalized is null)
        {
            _throttle.RegisterFailure(throttleKey);
            throw ApiException.InvalidPhone();
        }

        var customer = await FindOrCreateCustomerAsync(identity, normalized, request.FullName, cancellationToken);

        _throttle.Reset(throttleKey);

        var token = _jwt.CreateToken(customer.Id, RoleNames.Customer, customer.FullName, customer.TokenVersion, out var expiresAt);

        return new CustomerLoginResponse
        {
            Token = token,
            FullName = customer.FullName,
            PointsBalance = customer.PointsBalance,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>
    /// Builds the throttle key for an employee login attempt. Case- and
    /// whitespace-insensitive, so padding the username cannot buy a fresh counter.
    /// </summary>
    /// <param name="username">The username exactly as submitted.</param>
    /// <returns>The normalized, namespaced key.</returns>
    private static string EmployeeThrottleKey(string username) =>
        $"employee:{username.Trim().ToLowerInvariant()}";

    /// <summary>
    /// Builds the throttle key for a customer token exchange.
    /// </summary>
    /// <param name="uid">The Firebase uid from the verified token.</param>
    /// <param name="normalizedPhone">The E.164 phone number, or null when the token's phone claim is unusable.</param>
    /// <returns>The namespaced key. Namespacing keeps a phone number and a username from ever sharing a counter.</returns>
    private static string CustomerThrottleKey(string uid, string? normalizedPhone) =>
        normalizedPhone is not null
            ? $"customer:{normalizedPhone}"
            : $"customer-uid:{uid}";

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
            // A cashier may have registered this customer by phone only (blank name).
            // The app collects the name on first login; fill it in here when it is still blank.
            if (string.IsNullOrWhiteSpace(customer.FullName) && !string.IsNullOrWhiteSpace(fullName))
            {
                customer.FullName = fullName.Trim();
            }

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
    /// Attaches the Firebase uid on first link only. An already-linked customer keeps the
    /// uid they have: a mismatch is logged and the stored uid is left unchanged, but the
    /// login still proceeds and a token is issued. That is deliberate — Firebase has
    /// already proved ownership of this phone number by OTP, and a new uid on the same
    /// number is the normal outcome of re-registering the device or swapping the SIM.
    /// Blocking it would lock the legitimate owner out of their own account.
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
                "Customer {CustomerId} logged in with Firebase uid {IncomingUid} but is linked to a different uid; the login proceeds and the existing link is kept.",
                customer.Id,
                uid);
        }
    }
}
