namespace CoffeeLoyalty.Api.Enums;

public enum PointsTransactionType
{
    Earn,
    Redeem,
    Refund,
    RedeemReversal,

    /// <summary>One-time migration of a legacy paper-card balance; the only type with no justifying order.</summary>
    OpeningBalance
}
