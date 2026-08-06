namespace CoffeeLoyalty.Api.Constants;

/// <summary>
/// Loyalty rates in one place, not buried in formulas (per ERD "Rates").
/// </summary>
public static class LoyaltyConstants
{
    /// <summary>Points granted per dinar of cash paid (decision 37 — lowered from 5 at the shop owner's request).</summary>
    public const int PointsPerDinar = 3;

    /// <summary>Points redeemed per 1 JOD of discount.</summary>
    public const int RedeemRate = 100;

    /// <summary>Shop owner's minimum points per redemption.</summary>
    public const int MinRedeemPoints = 250;

    /// <summary>Days a return or cancellation is accepted after the order (Jordan time).</summary>
    public const int ReturnWindowDays = 1;

    /// <summary>
    /// Jordan's UTC offset, used to place an order in a calendar day before the return
    /// window is measured (decision 16).
    /// </summary>
    /// <remarks>
    /// A fixed offset rather than a <see cref="TimeZoneInfo"/> lookup, exactly as decision 16
    /// specifies: Jordan abolished DST in 2022, so <c>Asia/Amman</c> is permanently UTC+3.
    /// A lookup would also have to spell the zone differently per platform
    /// (<c>Asia/Amman</c> on Linux, <c>Jordan Standard Time</c> on Windows) and would drag in
    /// pre-2022 historical rules that no order in this system can ever fall under.
    /// </remarks>
    public static readonly TimeSpan JordanUtcOffset = TimeSpan.FromHours(3);
}
