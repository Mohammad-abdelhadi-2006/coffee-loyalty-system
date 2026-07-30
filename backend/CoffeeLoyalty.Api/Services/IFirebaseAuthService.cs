namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// The identity Firebase vouched for after OTP verification.
/// </summary>
/// <param name="Uid">The Firebase user id, stored on Customer.FirebaseUid (decision 11).</param>
/// <param name="PhoneNumber">The verified phone number as Firebase reports it (raw, not yet normalized).</param>
public sealed record FirebaseIdentity(string Uid, string? PhoneNumber);

/// <summary>
/// Thin wrapper over the Firebase Admin SDK, so the token check can be mocked in
/// tests and swapped without touching <see cref="IAuthService"/>.
/// </summary>
public interface IFirebaseAuthService
{
    /// <summary>
    /// Verifies a Firebase ID token and extracts the identity it carries.
    /// </summary>
    /// <param name="idToken">The raw ID token from the app.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The verified identity.</returns>
    /// <exception cref="Common.ApiException">401 <c>INVALID_FIREBASE_TOKEN</c> on any verification failure.</exception>
    Task<FirebaseIdentity> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}
