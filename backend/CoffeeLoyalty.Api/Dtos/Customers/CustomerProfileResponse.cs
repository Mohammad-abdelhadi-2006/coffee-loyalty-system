namespace CoffeeLoyalty.Api.Dtos.Customers;

/// <summary>
/// Body of <c>GET /api/customers/me</c> — what the app shows a customer about themselves
/// (api-contract.md §4).
/// </summary>
/// <remarks>
/// Narrower than <see cref="CustomerResponse"/> on purpose. The contract omits <c>id</c>
/// and <c>createdAt</c> here because the app has no use for them: it already knows who it
/// is signed in as, and its identity is the token, not a number it could send anywhere.
/// </remarks>
public class CustomerProfileResponse
{
    /// <summary>The customer's display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>The canonical E.164 number the account is keyed by.</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Current points balance.</summary>
    public int PointsBalance { get; set; }
}
