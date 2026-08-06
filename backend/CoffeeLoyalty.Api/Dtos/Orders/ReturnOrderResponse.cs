namespace CoffeeLoyalty.Api.Dtos.Orders;

/// <summary>
/// The body of <c>POST /api/orders/{id}/returns</c> — what this one return moved
/// (api-contract.md §5).
/// </summary>
/// <remarks>
/// Both figures describe <i>this</i> return, not the order's running totals. The order's
/// own <c>Total</c> and <c>PointsEarned</c> never change (decision 22); what has come back
/// so far is derived from each line's <c>ReturnedQuantity</c>.
/// </remarks>
public class ReturnOrderResponse
{
    /// <summary>Cash owed to the customer for the items in this request, in JOD.</summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Points taken back by this return, as a positive magnitude. Can be 0 when the
    /// returned value is too small to move the cumulative claw-back to the next whole point.
    /// </summary>
    public int PointsClawedBack { get; set; }

    /// <summary>The customer's balance once this return is committed.</summary>
    public int NewBalance { get; set; }
}
