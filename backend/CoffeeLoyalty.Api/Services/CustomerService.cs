using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Data;
using CoffeeLoyalty.Api.Dtos.Customers;
using CoffeeLoyalty.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// All customer-record rules live here; the controller only maps HTTP to these calls.
/// </summary>
public sealed class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CustomerService> _logger;

    /// <summary>
    /// Creates the service.
    /// </summary>
    /// <param name="db">The application database context.</param>
    /// <param name="logger">Diagnostics; never surfaced to the caller.</param>
    public CustomerService(AppDbContext db, ILogger<CustomerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CustomerResponse> RegisterAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        // Normalized first, so the duplicate check and the stored value are the same
        // string that a later app login will be matched against (decision 11).
        var phoneNumber = JordanPhoneNumber.NormalizeOrThrow(request.PhoneNumber);
        var fullName = NormalizeName(request.FullName);

        if (await _db.Customers.AnyAsync(c => c.PhoneNumber == phoneNumber, cancellationToken))
        {
            throw ApiException.PhoneAlreadyExists();
        }

        var customer = new Customer
        {
            FullName = fullName,
            PhoneNumber = phoneNumber,
            // Left null: a counter-registered customer has no Firebase account until
            // their first app login, which links the uid by phone number.
            FirebaseUid = null,
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
            // The check above is not a lock: two cashiers registering the same number at
            // once both pass it, and the unique index rejects the loser. That loser must
            // still get PHONE_ALREADY_EXISTS rather than a 500 — but only once the row is
            // confirmed to exist, so an unrelated write failure is not mislabelled.
            _db.Entry(customer).State = EntityState.Detached;

            if (!await _db.Customers.AnyAsync(c => c.PhoneNumber == phoneNumber, cancellationToken))
            {
                throw;
            }

            _logger.LogWarning(ex, "Concurrent registration of the same phone number; the later one is rejected.");

            throw ApiException.PhoneAlreadyExists();
        }

        return ToResponse(customer);
    }

    /// <inheritdoc />
    public async Task<CustomerResponse> FindByPhoneAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var normalized = JordanPhoneNumber.NormalizeOrThrow(phoneNumber);

        var customer = await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PhoneNumber == normalized, cancellationToken);

        return customer is not null
            ? ToResponse(customer)
            : throw ApiException.CustomerNotFound();
    }

    /// <inheritdoc />
    public async Task<CustomerProfileResponse> GetProfileAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        // Projected in SQL: FirebaseUid and TokenVersion are never read, let alone mapped.
        var profile = await _db.Customers
            .AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => new CustomerProfileResponse
            {
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                PointsBalance = c.PointsBalance
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Barely reachable: the token was validated against this row on the way in
        // (decision 25). It stays because "barely" is not "never" — a row deleted
        // mid-request must not become a NullReferenceException.
        return profile ?? throw ApiException.CustomerNotFound();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PointsTransactionResponse>> GetTransactionsAsync(
        int customerId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        // No existence check, unlike the cashier's routes: the id comes from the caller's own
        // validated token, so "no rows" here means an empty ledger and never a bad id. A new
        // customer with no movements yet gets [], which is the honest answer.
        return await _db.PointsTransactions
            .AsNoTracking()
            .Where(pt => pt.CustomerId == customerId)
            // Newest first by id — insertion order, and unlike CreatedAt it cannot tie
            // between the Redeem and the Earn written for the same order.
            .OrderByDescending(pt => pt.Id)
            .Take(limit)
            .Select(pt => new PointsTransactionResponse
            {
                Id = pt.Id,
                Type = pt.Type,
                Amount = pt.Amount,
                OrderId = pt.OrderId,
                CreatedAt = pt.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Trims the name and refuses one that is nothing but whitespace —
    /// <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> accepts <c>" "</c>.
    /// </summary>
    /// <param name="fullName">The submitted name.</param>
    /// <returns>The trimmed name.</returns>
    /// <exception cref="ApiException">400 <c>VALIDATION_ERROR</c> when nothing is left after trimming.</exception>
    private static string NormalizeName(string fullName)
    {
        var trimmed = fullName.Trim();

        return trimmed.Length > 0
            ? trimmed
            : throw ApiException.ValidationError("اسم العميل مطلوب");
    }

    /// <summary>
    /// Maps a customer row to the contract's customer object.
    /// </summary>
    /// <param name="customer">The entity.</param>
    /// <returns>The response DTO, without the internal columns.</returns>
    private static CustomerResponse ToResponse(Customer customer) => new()
    {
        Id = customer.Id,
        FullName = customer.FullName,
        PhoneNumber = customer.PhoneNumber,
        PointsBalance = customer.PointsBalance,
        CreatedAt = customer.CreatedAt
    };
}
