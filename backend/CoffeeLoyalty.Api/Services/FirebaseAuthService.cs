using CoffeeLoyalty.Api.Common;
using FirebaseAdmin;
using FirebaseAdmin.Auth;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// Verifies Firebase ID tokens with the Firebase Admin SDK initialised at startup.
/// </summary>
public sealed class FirebaseAuthService : IFirebaseAuthService
{
    /// <summary>The claim Firebase puts the OTP-verified phone number in.</summary>
    private const string PhoneNumberClaim = "phone_number";

    private readonly ILogger<FirebaseAuthService> _logger;

    /// <summary>
    /// Creates the verifier.
    /// </summary>
    /// <param name="logger">Logs the real reason a token was rejected; clients only ever see the generic code.</param>
    public FirebaseAuthService(ILogger<FirebaseAuthService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<FirebaseIdentity> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw ApiException.InvalidFirebaseToken();
        }

        if (FirebaseApp.DefaultInstance is null)
        {
            // Startup logged the reason; surfacing anything more here would tell an
            // anonymous caller about our server configuration.
            _logger.LogError("Firebase Admin SDK is not initialised — customer login is unavailable.");
            throw ApiException.InvalidFirebaseToken();
        }

        FirebaseToken token;
        try
        {
            token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken, cancellationToken);
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogWarning(ex, "Firebase ID token verification failed.");
            throw ApiException.InvalidFirebaseToken();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected failure while verifying a Firebase ID token.");
            throw ApiException.InvalidFirebaseToken();
        }

        var phoneNumber = token.Claims.TryGetValue(PhoneNumberClaim, out var claim)
            ? claim?.ToString()
            : null;

        return new FirebaseIdentity(token.Uid, phoneNumber);
    }
}
