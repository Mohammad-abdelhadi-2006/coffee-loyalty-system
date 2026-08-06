using System.ComponentModel.DataAnnotations;

namespace CoffeeLoyalty.Api.Dtos.Orders;

/// <summary>
/// Body of <c>POST /api/orders</c> (api-contract.md §5) — the cashier's basket.
/// </summary>
/// <remarks>
/// Deliberately the smallest possible body: who is buying, how many points they want to
/// spend, and what is in the basket. Every number that decides money or points — unit
/// price, line value, total, cash paid, points earned — is computed by the server from
/// the catalog. Nothing a client sends can influence them.
/// </remarks>
public class CreateOrderRequest
{
    /// <summary>The customer earning and (optionally) spending the points.</summary>
    [Required]
    public int? CustomerId { get; set; }

    /// <summary>
    /// Points to spend on this order. Omitted or <c>null</c> means 0 — a plain cash sale.
    /// </summary>
    /// <remarks>
    /// The redemption rules themselves (a 250-point minimum, a balance that covers it, and a
    /// value not exceeding the order total) live in the service, because each one needs
    /// either the catalog or the customer's row; the <see cref="RangeAttribute"/> here only
    /// rules out a negative number, which would otherwise *add* points at the till.
    /// </remarks>
    [Range(0, int.MaxValue)]
    public int? PointsRedeemed { get; set; }

    /// <summary>The basket. An order with no lines is not an order.</summary>
    [Required]
    [MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; set; } = [];
}
