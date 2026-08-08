namespace CoffeeLoyalty.Api.Options;

/// <summary>
/// The <c>Cors</c> configuration section (decision 41). Nothing here is secret — an allowed
/// origin is public the moment a browser sends a request — but every value is
/// deployment-specific, so production supplies them through environment configuration
/// (<c>Cors__AllowedOrigins__0</c>, <c>Cors__AllowedOrigins__1</c>, …) and the committed
/// files carry only the local development origins.
/// </summary>
public sealed class CorsOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Browser origins allowed to call the API. An origin is a scheme, a host and an optional
    /// port and nothing else — <c>https://pos.example.com</c>, never a path and never a
    /// trailing slash, because that is the exact form the browser puts in the <c>Origin</c>
    /// header and the match is a string comparison.
    /// </summary>
    /// <remarks>
    /// Empty is a legal configuration, not a failure: the Flutter app is not a browser, sends
    /// no <c>Origin</c>, and is unaffected by any of this. It does mean the dashboard is
    /// blocked, which is why startup says so out loud rather than leaving it to be discovered
    /// in a browser console.
    /// </remarks>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// The configured origins in the form the CORS middleware compares against: blanks
    /// dropped, surrounding whitespace and any trailing slash trimmed, duplicates removed.
    /// </summary>
    /// <returns>The cleaned origins; an empty array when none are configured.</returns>
    /// <remarks>
    /// The trailing slash is trimmed rather than rejected because it is the mistake this
    /// setting actually attracts: <c>https://pos.example.com/</c> is what someone copies out
    /// of a browser's address bar, it reads as correct in a deployment panel, and it matches
    /// nothing — the failure then surfaces only as a CORS error in the dashboard, with nothing
    /// on the server side to explain it.
    /// </remarks>
    public string[] NormalizedOrigins() =>
        [.. AllowedOrigins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)];
}
