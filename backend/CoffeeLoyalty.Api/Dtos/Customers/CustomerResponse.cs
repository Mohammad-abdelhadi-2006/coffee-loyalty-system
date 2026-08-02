namespace CoffeeLoyalty.Api.Dtos.Customers;

/// <summary>
/// The customer object the counter sees: the body of <c>POST /api/customers</c> and of
/// the phone lookup (api-contract.md §4).
/// </summary>
/// <remarks>
/// Deliberately not the entity: <c>FirebaseUid</c> and <c>TokenVersion</c> are internal
/// and are exposed by no endpoint.
/// </remarks>
public class CustomerResponse
{
    /// <summary>Primary key; what the orders module will reference.</summary>
    public int Id { get; set; }

    /// <summary>The customer's display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>The canonical E.164 number (<c>+9627XXXXXXXX</c>), not the spelling that was submitted.</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Current points balance. Zero for a customer the cashier just registered.</summary>
    public int PointsBalance { get; set; }

    /// <summary>When the customer was registered (UTC).</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
