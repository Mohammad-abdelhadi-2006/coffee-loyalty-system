using System.ComponentModel.DataAnnotations;

namespace CoffeeLoyalty.Api.Dtos.Orders;

/// <summary>
/// Body of <c>POST /api/orders/{id}/returns</c> — which lines are coming back, and how
/// much of each (api-contract.md §5).
/// </summary>
public class ReturnOrderRequest
{
    /// <summary>The lines being returned. A return that names nothing is not a return.</summary>
    [Required]
    [MinLength(1)]
    public List<ReturnOrderItemRequest> Items { get; set; } = [];
}

/// <summary>
/// One line of a return: an OrderItem of this order, and how much of it comes back.
/// </summary>
/// <remarks>
/// The line is named by <c>orderItemId</c>, not by product. Two lines of the same product
/// stay separate on the order (a cashier who rang it twice described two lines), so a
/// return has to say which one it means.
/// </remarks>
public class ReturnOrderItemRequest
{
    /// <summary>The OrderItem coming back. Must belong to the order in the route.</summary>
    [Required]
    public int? OrderItemId { get; set; }

    /// <summary>
    /// How much of that line comes back, strictly positive and no more than what is left
    /// on it. The upper bound mirrors the order DTO's, so a mistyped quantity is a
    /// validation failure rather than an overflow.
    /// </summary>
    [Required]
    [Range(typeof(decimal), "0.001", "999999.999",
        ConvertValueInInvariantCulture = true,
        ParseLimitsInInvariantCulture = true)]
    public decimal? Quantity { get; set; }
}
