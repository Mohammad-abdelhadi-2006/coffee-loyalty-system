using System.ComponentModel.DataAnnotations;

namespace CoffeeLoyalty.Api.Dtos.Orders;

/// <summary>
/// One line of <c>POST /api/orders</c>. Carries a product id and a quantity and
/// nothing else — no name, no price. The server reads both from the catalog and
/// snapshots them onto the OrderItem (decision 3).
/// </summary>
/// <remarks>
/// Both properties are nullable for the same reason the product DTO's enums are: a
/// non-nullable <c>int</c> has no "absent" state, so an omitted <c>productId</c> would
/// bind to <c>0</c> and be looked up as if the cashier had sent it. Nullable plus
/// <see cref="RequiredAttribute"/> makes the omission a <c>VALIDATION_ERROR</c>.
/// </remarks>
public class CreateOrderItemRequest
{
    /// <summary>The catalog product being sold.</summary>
    [Required]
    public int? ProductId { get; set; }

    /// <summary>
    /// How much of it, strictly positive. Decimal because a quantity is a count for a
    /// <c>Piece</c> product and a weight for a <c>Kg</c> one. The upper bound keeps a
    /// mistyped quantity from overflowing the order total's <c>decimal(18,3)</c> column
    /// as an unhandled 500; it is far above anything a counter sells in one line.
    /// </summary>
    /// <remarks>
    /// The three rules the <see cref="RangeAttribute"/> cannot express — no more than three
    /// decimals, and a whole number when the product is sold by the piece — are checked in
    /// the service and reported as <c>INVALID_QUANTITY</c>, because both need the catalog row.
    /// </remarks>
    [Required]
    [Range(typeof(decimal), "0.001", "999999.999",
        ConvertValueInInvariantCulture = true,
        ParseLimitsInInvariantCulture = true)]
    public decimal? Quantity { get; set; }
}
