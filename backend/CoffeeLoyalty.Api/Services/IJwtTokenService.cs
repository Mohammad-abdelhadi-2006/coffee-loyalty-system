namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// Issues the API's own JWTs. Employees and customers both end up carrying one of
/// these, so authorization deals with a single token type (decision 5).
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Signs a token for one user.
    /// </summary>
    /// <param name="userId">Employee id or customer id; becomes the <c>sub</c> claim.</param>
    /// <param name="role">A canonical lowercase role string from <see cref="Constants.RoleNames"/>.</param>
    /// <param name="fullName">Display name; becomes the <c>name</c> claim.</param>
    /// <param name="expiresAt">Receives the moment the token stops being valid (UTC).</param>
    /// <returns>The compact-serialized, signed JWT.</returns>
    string CreateToken(int userId, string role, string fullName, out DateTimeOffset expiresAt);
}
