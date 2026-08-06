namespace CoffeeLoyalty.Api.Dtos.Orders;

/// <summary>
/// One line of an order as it is read back (api-contract.md §4/§5).
/// </summary>
/// <remarks>
/// The name and price are the snapshots taken when the order was rung up, not the
/// catalog's current values (decision 3) — a renamed or repriced product never rewrites
/// a past receipt. There is no <c>productId</c> here on purpose: the contract's shape
/// does not carry one, and nothing on the returns screen needs it. What the screen does
/// need is <see cref="OrderItemId"/>, which is the handle
/// <c>POST /api/orders/{id}/returns</c> takes.
/// </remarks>
public class OrderItemResponse
{
    /// <summary>The OrderItem's id — what a return request names.</summary>
    public int OrderItemId { get; set; }

    /// <summary>The product's name as it was at order time.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>How much was ordered. A count for a <c>Piece</c> product, a weight for a <c>Kg</c> one.</summary>
    public decimal Quantity { get; set; }

    /// <summary>How much of it has come back so far; 0 until the first return on this line.</summary>
    public decimal ReturnedQuantity { get; set; }

    /// <summary>The unit price as it was at order time.</summary>
    public decimal UnitPriceSnapshot { get; set; }
}
