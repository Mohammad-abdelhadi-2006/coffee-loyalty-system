namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// Password hashing verification, behind an interface so the login path's
/// timing-equalization behaviour can be observed in tests without measuring
/// wall-clock time.
/// </summary>
public interface IPasswordVerifier
{
    /// <summary>
    /// Verifies a plaintext password against a stored hash.
    /// </summary>
    /// <param name="password">The submitted plaintext password.</param>
    /// <param name="passwordHash">The stored BCrypt hash to verify against.</param>
    /// <returns><c>true</c> when the password matches.</returns>
    bool Verify(string password, string passwordHash);
}
