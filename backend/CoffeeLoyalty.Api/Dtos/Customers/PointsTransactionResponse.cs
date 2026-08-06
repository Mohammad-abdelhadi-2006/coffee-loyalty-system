using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Dtos.Customers;

/// <summary>
/// One line of the customer's points ledger — the body of
/// <c>GET /api/customers/me/transactions</c> (api-contract.md §4).
/// </summary>
public class PointsTransactionResponse
{
    /// <summary>The transaction's id.</summary>
    public int Id { get; set; }

    /// <summary>
    /// Why the balance moved: <c>Earn</c>, <c>Redeem</c>, <c>Refund</c>,
    /// <c>RedeemReversal</c> or <c>OpeningBalance</c>. It describes the reason, never the
    /// sign — read <see cref="Amount"/> for that.
    /// </summary>
    public PointsTransactionType Type { get; set; }

    /// <summary>The effect on the balance: positive increases it, negative decreases it.</summary>
    public int Amount { get; set; }

    /// <summary>
    /// The order that justifies the movement. <c>null</c> only on <c>OpeningBalance</c>,
    /// the one-time import of a legacy paper-card balance (decision 38); every other type
    /// has one, enforced by a database check constraint.
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>When the movement happened.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
