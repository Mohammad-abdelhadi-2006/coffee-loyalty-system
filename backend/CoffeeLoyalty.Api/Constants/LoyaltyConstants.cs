namespace CoffeeLoyalty.Api.Constants;

/// <summary>
/// Loyalty rates in one place, not buried in formulas (per ERD "Rates").
/// </summary>
public static class LoyaltyConstants
{
    /// <summary>Points granted per dinar of cash paid.</summary>
    public const int PointsPerDinar = 5;

    /// <summary>Points redeemed per 1 JOD of discount.</summary>
    public const int RedeemRate = 100;

    /// <summary>Shop owner's minimum points per redemption.</summary>
    public const int MinRedeemPoints = 250;

    /// <summary>Days a return or cancellation is accepted after the order (Jordan time).</summary>
    public const int ReturnWindowDays = 1;
}
