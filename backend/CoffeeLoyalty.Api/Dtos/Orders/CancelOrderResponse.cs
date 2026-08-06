namespace CoffeeLoyalty.Api.Dtos.Orders;

/// <summary>
/// The body of <c>POST /api/orders/{id}/cancel</c> — what the cancellation moved
/// (api-contract.md §5).
/// </summary>
/// <remarks>
/// Both point figures are reported as positive magnitudes, describing the direction in
/// their names rather than in a sign. The ledger rows behind them are signed: the
/// <c>Refund</c> is negative, the <c>RedeemReversal</c> positive.
/// </remarks>
public class CancelOrderResponse
{
    /// <summary>Points taken back, equal to the order's <c>PointsEarned</c>.</summary>
    public int PointsClawedBack { get; set; }

    /// <summary>Points handed back, equal to the order's <c>PointsRedeemed</c> (decision 6).</summary>
    public int PointsRestored { get; set; }

    /// <summary>The customer's balance once both movements are committed.</summary>
    public int NewBalance { get; set; }
}
