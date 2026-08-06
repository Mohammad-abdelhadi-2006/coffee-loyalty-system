namespace CoffeeLoyalty.Api.Dtos.Orders;

/// <summary>
/// The body of <c>GET /api/orders/{id}</c>: the same order shape the customer's list
/// returns, plus who bought it and who rang it up (api-contract.md §5).
/// </summary>
/// <remarks>
/// Inherits rather than redeclares, so "same shape plus three fields" is a fact about the
/// code and not a convention two DTOs have to keep agreeing on by hand.
/// <para>
/// The two names are read live from the Customer and Employee rows, not snapshotted like
/// the product name. They are not part of what was sold: a renamed product would falsify
/// a receipt, whereas an employee who changed their display name is still the same person
/// who served that order, and showing their current name is the useful answer.
/// </para>
/// </remarks>
public class OrderDetailResponse : OrderResponse
{
    /// <summary>The customer who bought it.</summary>
    public int CustomerId { get; set; }

    /// <summary>That customer's current display name.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>The cashier who rang it up, by current display name.</summary>
    public string EmployeeName { get; set; } = string.Empty;
}
