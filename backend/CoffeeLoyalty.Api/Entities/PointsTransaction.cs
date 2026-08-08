using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Entities;

public class PointsTransaction
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    /// <summary>Required for every type except OpeningBalance: a points movement has a justifying order — no sourceless
    /// adjustments (decision 12). Only the one-time legacy import leaves it NULL; enforced by CK_PointsTransaction_Order.</summary>
    public int? OrderId { get; set; }

    /// <summary>Signed effect on the balance (positive = increase, negative = decrease); Type describes the reason, not the sign.</summary>
    public int Amount { get; set; }

    /// <summary>Earn / Redeem / Refund / RedeemReversal; kept separate so reports can distinguish them.
    /// Stored as its member name, not its ordinal — see the HasConversion&lt;string&gt;() mapping in AppDbContext.OnModelCreating,
    /// which is also what the CK_PointsTransaction_Order and UX_PointsTransaction_Order_Type filters compare against.</summary>
    public PointsTransactionType Type { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Order? Order { get; set; }
}
