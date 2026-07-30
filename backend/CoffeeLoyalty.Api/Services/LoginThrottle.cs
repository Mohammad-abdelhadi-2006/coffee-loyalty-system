using CoffeeLoyalty.Api.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// In-process <see cref="ILoginThrottle"/> backed by <see cref="IMemoryCache"/>.
/// Counts live in this instance's memory only — see decision 27 for what that costs.
/// </summary>
public sealed class LoginThrottle : ILoginThrottle
{
    /// <summary>Namespaces the cache, which is shared with whatever else caches here.</summary>
    private const string CacheKeyPrefix = "login-throttle:";

    private readonly IMemoryCache _cache;
    private readonly LoginThrottleOptions _options;
    private readonly ILogger<LoginThrottle> _logger;

    /// <summary>Serializes the read-modify-write on a counter; <see cref="IMemoryCache"/> gives no atomic increment.</summary>
    private readonly Lock _gate = new();

    /// <summary>
    /// Creates the throttle.
    /// </summary>
    /// <param name="cache">The process-wide memory cache.</param>
    /// <param name="options">The validated <c>LoginThrottle</c> section.</param>
    /// <param name="logger">Records blocks so a locked-out user can be explained after the fact.</param>
    public LoginThrottle(IMemoryCache cache, IOptions<LoginThrottleOptions> options, ILogger<LoginThrottle> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsBlocked(string key)
    {
        lock (_gate)
        {
            return _cache.TryGetValue(CacheKeyPrefix + key, out FailureCount? record)
                && record is not null
                && record.Failures >= _options.MaxFailures;
        }
    }

    /// <inheritdoc />
    public void RegisterFailure(string key)
    {
        var cacheKey = CacheKeyPrefix + key;

        lock (_gate)
        {
            var record = _cache.TryGetValue(cacheKey, out FailureCount? existing) && existing is not null
                ? existing
                : new FailureCount();

            record.Failures++;

            var blocked = record.Failures >= _options.MaxFailures;

            // Below the threshold the entry expires on inactivity, which is what makes
            // the window rolling. At the threshold it switches to a fixed lifetime, so
            // the block ends at a predictable moment instead of being extended by the
            // attacker's own retries.
            var entryOptions = blocked
                ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = _options.BlockDuration }
                : new MemoryCacheEntryOptions { SlidingExpiration = _options.FailureWindow };

            _cache.Set(cacheKey, record, entryOptions);

            if (blocked)
            {
                // The key is an account identifier, never a password.
                _logger.LogWarning(
                    "Authentication for {ThrottleKey} is blocked for {BlockMinutes} minutes after {Failures} failed attempts.",
                    key,
                    _options.BlockMinutes,
                    record.Failures);
            }
        }
    }

    /// <inheritdoc />
    public void Reset(string key)
    {
        lock (_gate)
        {
            _cache.Remove(CacheKeyPrefix + key);
        }
    }

    /// <summary>
    /// Mutable counter held in the cache, so a failure updates in place instead of
    /// replacing a boxed value.
    /// </summary>
    private sealed class FailureCount
    {
        /// <summary>Failed attempts recorded so far for one identifier.</summary>
        public int Failures { get; set; }
    }
}
