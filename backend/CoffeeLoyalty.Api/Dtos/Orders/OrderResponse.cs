using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Dtos.Orders;

/// <summary>
/// An order with its lines — the shape <c>GET /api/customers/{id}/orders</c> returns,
/// and the base of the single-order shape (api-contract.md §4).
/// </summary>
/// <remarks>
/// <see cref="Total"/>, <see cref="PointsEarned"/> and <see cref="PointsRedeemed"/> are
/// the figures recorded at creation and are never rewritten by a return or a cancellation
/// (decision 22). A client showing "what is this order worth now" derives it from the
/// items' <see cref="OrderItemResponse.ReturnedQuantity"/>; the server does not store it.
/// </remarks>
public class OrderResponse
{
    /// <summary>The order's id.</summary>
    public int OrderId { get; set; }

    /// <summary>When it was rung up.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// <c>Completed</c>, <c>Returned</c> or <c>Cancelled</c>. <c>Returned</c> is one-way and
    /// says nothing about how much came back — that is derived from the items.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>Full order value at creation, in JOD.</summary>
    public decimal Total { get; set; }

    /// <summary>Points spent on this order.</summary>
    public int PointsRedeemed { get; set; }

    /// <summary>Points granted by this order, on cash paid only (decision 9).</summary>
    public int PointsEarned { get; set; }

    /// <summary>The lines, in the order they were rung up.</summary>
    public List<OrderItemResponse> Items { get; set; } = [];
}
