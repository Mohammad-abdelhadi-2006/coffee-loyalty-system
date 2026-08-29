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
    /// <param name="contentRootPath">The application's content root; anything that is not a
    /// fully qualified path is resolved against it.</param>
    /// <exception cref="InvalidOperationException">A path is configured but no file is there.</exception>
    public static void Initialize(IConfiguration configuration, ILogger logger, string contentRootPath)
    {
        if (FirebaseApp.DefaultInstance is not null)
        {
            return;
        }

        var configuredPath = configuration[CredentialsPathKey];

        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            logger.LogWarning(
                "{Key} is not configured — POST /api/auth/firebase-login will reject every token. " +
                "Set it to the path of the Firebase service-account JSON (kept outside the repository).",
                CredentialsPathKey);
            return;
        }

        var credentialsPath = Resolve(configuredPath, contentRootPath);

        if (!File.Exists(credentialsPath))
        {
            // Both forms, because they differ exactly when the configured value is the thing
            // that is wrong — a Unix-style '/private/x.json' typed into a Windows host is the
            // case this message exists for, and seeing where it actually looked is the whole
            // diagnosis.
            throw new InvalidOperationException(
                $"{CredentialsPathKey} is set to '{configuredPath}', which resolved to " +
                $"'{credentialsPath}' — no file is there. Give either a fully qualified path " +
                $"(e.g. 'C:\\path\\to\\firebase.json') or one relative to the content root " +
                $"('{contentRootPath}'), and upload the service-account JSON outside the webroot.");
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

        logger.LogInformation("Firebase Admin SDK initialised from {Path}.", credentialsPath);
    }

    /// <summary>
    /// Turns a configured path into one the file system can be asked about.
    /// </summary>
    /// <remarks>
    /// The test is <see cref="Path.IsPathFullyQualified(string)"/>, not
    /// <c>Path.IsPathRooted</c>, and the difference is the entire point on Windows.
    /// <c>IsPathRooted("/private/x.json")</c> is <c>true</c> there — a leading slash roots the
    /// path on the *current drive*, so the file is looked for at <c>C:\private\x.json</c>,
    /// wherever the site actually lives. A hosting panel that writes its paths Unix-style
    /// therefore produces a value that is silently wrong rather than obviously wrong.
    /// <para>
    /// Treating such a path as relative to the content root is the reading that matches what
    /// someone means by "/private/firebase.json" on a shared host: the folder beside the
    /// application, not the root of the server's system drive.
    /// </para>
    /// </remarks>
    private static string Resolve(string configuredPath, string contentRootPath)
    {
        var trimmed = configuredPath.Trim();

        if (Path.IsPathFullyQualified(trimmed))
        {
            return Path.GetFullPath(trimmed);
        }

        return Path.GetFullPath(
            Path.Combine(contentRootPath, trimmed.TrimStart('/', '\\')));
    }
}
