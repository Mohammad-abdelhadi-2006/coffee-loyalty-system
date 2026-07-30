namespace CoffeeLoyalty.Api.Options;

/// <summary>
/// The <c>LoginThrottle</c> configuration section (decision 27). Nothing here is
/// sensitive, so unlike <see cref="JwtOptions"/> these values live in a committed
/// appsettings file; they are tuning knobs the shop may want to loosen or tighten.
/// </summary>
public sealed class LoginThrottleOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "LoginThrottle";

    /// <summary>
    /// Failed attempts against one identifier that trigger a block. Reached, not exceeded:
    /// the attempt that hits this number is the last one accepted.
    /// </summary>
    public int MaxFailures { get; set; } = 5;

    /// <summary>
    /// The rolling window failures are counted over, in minutes. Idle time longer than
    /// this clears the count, so an honest user who mistypes twice a day is never blocked.
    /// </summary>
    public int FailureWindowMinutes { get; set; } = 15;

    /// <summary>
    /// How long an identifier stays blocked once <see cref="MaxFailures"/> is reached,
    /// in minutes. The block lifts on its own; there is no unlock endpoint.
    /// </summary>
    public int BlockMinutes { get; set; } = 15;

    /// <summary>The rolling failure window as a <see cref="TimeSpan"/>.</summary>
    public TimeSpan FailureWindow => TimeSpan.FromMinutes(FailureWindowMinutes);

    /// <summary>The block duration as a <see cref="TimeSpan"/>.</summary>
    public TimeSpan BlockDuration => TimeSpan.FromMinutes(BlockMinutes);

    /// <summary>
    /// Fails fast on a misconfigured throttle. A zero or negative value here would
    /// either block every login or disable the protection silently — both are worse
    /// than refusing to start.
    /// </summary>
    /// <exception cref="InvalidOperationException">Any value is zero or negative.</exception>
    public void Validate()
    {
        if (MaxFailures <= 0)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(MaxFailures)} must be a positive number of attempts.");
        }

        if (FailureWindowMinutes <= 0)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(FailureWindowMinutes)} must be a positive number of minutes.");
        }

        if (BlockMinutes <= 0)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(BlockMinutes)} must be a positive number of minutes.");
        }
    }
}
