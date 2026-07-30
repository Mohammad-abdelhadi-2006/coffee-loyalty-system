namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// The production <see cref="IPasswordVerifier"/>: BCrypt at the work factor the
/// library defaults to, matching the hashes written by the admin seeder.
/// </summary>
public sealed class BCryptPasswordVerifier : IPasswordVerifier
{
    private readonly ILogger<BCryptPasswordVerifier> _logger;

    /// <summary>
    /// Creates the verifier.
    /// </summary>
    /// <param name="logger">Records unreadable stored hashes; never surfaced to the caller.</param>
    public BCryptPasswordVerifier(ILogger<BCryptPasswordVerifier> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// A corrupt or legacy hash is treated as a failed login rather than a server
    /// error — the caller must not be able to tell a broken row from a wrong password.
    /// </remarks>
    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException ex)
        {
            _logger.LogError(ex, "A stored password hash is unreadable.");
            return false;
        }
    }
}
