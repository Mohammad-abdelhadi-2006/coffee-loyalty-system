namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// Counts failed authentication attempts per account identifier and blocks an
/// identifier that fails too often (decision 27). Keyed on the submitted identifier
/// rather than the caller's IP, so it works unchanged behind a reverse proxy.
/// </summary>
/// <remarks>
/// Callers must count a failure for identifiers that do not exist exactly as they do
/// for ones that do. Skipping the unknown case would make "was I throttled?" a test
/// for whether an account exists.
/// </remarks>
public interface ILoginThrottle
{
    /// <summary>
    /// Whether this identifier is currently blocked from authenticating.
    /// </summary>
    /// <param name="key">The account identifier, already normalized by the caller.</param>
    /// <returns><c>true</c> while the block lasts.</returns>
    bool IsBlocked(string key);

    /// <summary>
    /// Records one failed attempt, blocking the identifier once the configured
    /// threshold is reached.
    /// </summary>
    /// <param name="key">The account identifier, already normalized by the caller.</param>
    void RegisterFailure(string key);

    /// <summary>
    /// Clears everything recorded for this identifier. Called after a successful
    /// authentication, so an honest user's earlier typos never accumulate into a block.
    /// </summary>
    /// <param name="key">The account identifier, already normalized by the caller.</param>
    void Reset(string key);
}
