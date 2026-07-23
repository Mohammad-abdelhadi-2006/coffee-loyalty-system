using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Entities;

public class PointsTransaction
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    /// <summary>NOT NULL: every points movement has a justifying order — no sourceless adjustments (decision 12).</summary>
    public int OrderId { get; set; }

    /// <summary>Signed effect on the balance (positive = increase, negative = decrease); Type describes the reason, not the sign.</summary>
    public int Amount { get; set; }

    // TODO: stored as string via HasConversion<string>() in Fluent API.
    /// <summary>Earn / Redeem / Refund / RedeemReversal; kept separate so reports can distinguish them.</summary>
    public PointsTransactionType Type { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
