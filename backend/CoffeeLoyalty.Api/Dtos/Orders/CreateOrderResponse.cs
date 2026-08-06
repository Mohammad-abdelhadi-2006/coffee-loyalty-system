namespace CoffeeLoyalty.Api.Dtos.Orders;

/// <summary>
/// The 201 body of <c>POST /api/orders</c> — the receipt line the cashier reads back
/// to the customer.
/// </summary>
/// <remarks>
/// <see cref="CashPaid"/> is derived, never stored (decision 10): it is
/// <see cref="Total"/> minus the redeemed points' value. It is returned anyway because
/// it is the number actually collected at the till, and no client should have to
/// re-implement the formula to display it.
/// </remarks>
public class CreateOrderResponse
{
    /// <summary>The new order's id — the handle for a later return or cancellation.</summary>
    public int OrderId { get; set; }

    /// <summary>Full order value in JOD, the sum of the lines as ordered.</summary>
    public decimal Total { get; set; }

    /// <summary>What the customer pays in cash: <c>Total − PointsRedeemed / 100</c>.</summary>
    public decimal CashPaid { get; set; }

    /// <summary>Points spent on this order; 0 for a plain cash sale.</summary>
    public int PointsRedeemed { get; set; }

    /// <summary>Points granted, computed on <see cref="CashPaid"/> only (decision 9).</summary>
    public int PointsEarned { get; set; }

    /// <summary>The customer's balance once this order's points movements are committed.</summary>
    public int NewBalance { get; set; }
}
