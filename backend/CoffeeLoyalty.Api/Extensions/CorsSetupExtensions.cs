using CoffeeLoyalty.Api.Options;

namespace CoffeeLoyalty.Api.Extensions;

/// <summary>
/// Cross-origin setup for the React dashboard (decision 41), kept out of Program.cs the same
/// way <see cref="AuthSetupExtensions.AddCoffeeLoyaltyAuth"/> is.
/// </summary>
/// <remarks>
/// This exists for exactly one client. The dashboard runs in a browser on the shop's POS
/// machine and calls an API hosted elsewhere, so every request it makes is cross-origin and
/// the browser blocks the response unless the server opts in. The Flutter app enforces no
/// same-origin policy and is unaffected by anything in this file.
/// </remarks>
public static class CorsSetupExtensions
{
    /// <summary>
    /// The one named policy. Named rather than default so the pipeline states which policy it
    /// applies, and so adding a second one later cannot silently change this one.
    /// </summary>
    public const string PolicyName = "CoffeeLoyaltyDashboard";

    /// <summary>
    /// Registers the dashboard's CORS policy from the <c>Cors</c> configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (appsettings / environment).</param>
    /// <returns>The same collection, for chaining.</returns>
    /// <remarks>
    /// Origins are listed explicitly and credentials are never allowed. The API authenticates
    /// with a Bearer token in the <c>Authorization</c> header (decision 15), not a cookie, so
    /// the browser never needs credential mode — and <c>AllowAnyOrigin</c> together with
    /// <c>AllowCredentials</c> is rejected by the framework in any case. Any header and any
    /// method are allowed because the contract already uses several of both and the origin is
    /// the control that matters.
    /// <para>
    /// A missing or empty section is not a startup failure — see
    /// <see cref="WarnIfNoOriginsConfigured"/>. It produces a policy that matches no origin, so
    /// no <c>Access-Control-Allow-Origin</c> header is ever emitted and browsers block the
    /// dashboard exactly as they do without any CORS support at all.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddCoffeeLoyaltyCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = ReadOrigins(configuration);

        services.AddCors(options => options.AddPolicy(
            PolicyName,
            policy => policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()));

        return services;
    }

    /// <summary>
    /// Reports the policy at startup, and warns when it would block every browser client.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Startup logger.</param>
    /// <remarks>
    /// A warning rather than a throw, deliberately: an API serving only the Flutter app is a
    /// valid deployment, so refusing to start would break a working setup to complain about an
    /// unused feature. But an empty list that says nothing is the worst outcome — the dashboard
    /// then fails in the browser with a message naming CORS, and the server logs hold nothing
    /// to correlate it with. This line is that correlation.
    /// </remarks>
    public static void WarnIfNoOriginsConfigured(IConfiguration configuration, ILogger logger)
    {
        var origins = ReadOrigins(configuration);
        var key = $"{CorsOptions.SectionName}:{nameof(CorsOptions.AllowedOrigins)}";

        if (origins.Length == 0)
        {
            logger.LogWarning(
                "{Key} is empty — every browser request from the dashboard will be blocked by CORS. " +
                "Set the dashboard's origin exactly as the browser sends it (scheme + host + port, " +
                "no trailing slash and no path), e.g. Cors__AllowedOrigins__0=https://dashboard.example.com. " +
                "The Flutter app is not a browser and is unaffected.",
                key);

            return;
        }

        logger.LogInformation(
            "CORS policy '{Policy}' allows {Count} origin(s): {Origins}.",
            PolicyName,
            origins.Length,
            string.Join(", ", origins));
    }

    /// <summary>Binds the section and cleans the configured origins.</summary>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The origins in the form the middleware compares against.</returns>
    private static string[] ReadOrigins(IConfiguration configuration) =>
        (configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions())
            .NormalizedOrigins();
}
