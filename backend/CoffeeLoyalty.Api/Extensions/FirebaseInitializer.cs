using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace CoffeeLoyalty.Api.Extensions;

/// <summary>
/// Brings up the Firebase Admin SDK once per process. The service-account JSON is a
/// credential: its path is configured, and the file itself lives outside the
/// repository (or in an ignored path) and is never committed.
/// </summary>
public static class FirebaseInitializer
{
    /// <summary>Configuration key holding the path to the service-account JSON.</summary>
    public const string CredentialsPathKey = "Firebase:CredentialsPath";

    /// <summary>
    /// Creates the default <see cref="FirebaseApp"/> if it does not exist yet.
    /// A configured-but-missing file is a misconfiguration and stops startup; no
    /// configuration at all only disables customer login, which is logged loudly so
    /// a developer without the JSON can still work on the dashboard side.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Startup logger.</param>
    /// <exception cref="InvalidOperationException">A path is configured but no file is there.</exception>
    public static void Initialize(IConfiguration configuration, ILogger logger)
    {
        if (FirebaseApp.DefaultInstance is not null)
        {
            return;
        }

        var credentialsPath = configuration[CredentialsPathKey];

        if (string.IsNullOrWhiteSpace(credentialsPath))
        {
            logger.LogWarning(
                "{Key} is not configured — POST /api/auth/firebase-login will reject every token. " +
                "Set it to the path of the Firebase service-account JSON (kept outside the repository).",
                CredentialsPathKey);
            return;
        }

        if (!File.Exists(credentialsPath))
        {
            throw new InvalidOperationException(
                $"{CredentialsPathKey} points to '{credentialsPath}', which does not exist. " +
                "Download the service-account JSON from the Firebase console and point this key at it.");
        }

        // FromFile is marked obsolete in favour of CredentialFactory, which does not
        // exist in the Google.Apis.Auth version this project resolves — so there is no
        // migration target yet. Revisit when the dependency is next upgraded.
#pragma warning disable CS0618
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(credentialsPath)
        });
#pragma warning restore CS0618

        logger.LogInformation("Firebase Admin SDK initialised.");
    }
}
