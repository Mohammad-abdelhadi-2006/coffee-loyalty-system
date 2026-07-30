namespace CoffeeLoyalty.Api.Options;

/// <summary>
/// The <c>Jwt</c> configuration section. <see cref="Secret"/> is sensitive and comes
/// from User Secrets in development / an environment variable in production —
/// it is never written into a committed appsettings file.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>HMAC-SHA256 signing key. Must be at least 32 characters (256 bits).</summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>The <c>iss</c> claim, validated on every request.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>The <c>aud</c> claim, validated on every request.</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Employee token lifetime in hours (decision 15): short, because staff log in at
    /// the start of a shift and there are no refresh tokens to revoke.
    /// </summary>
    public int EmployeeExpireHours { get; set; } = 12;

    /// <summary>
    /// Customer token lifetime in days (decision 15): long, because every expiry costs
    /// the customer another OTP round-trip.
    /// </summary>
    public int CustomerExpireDays { get; set; } = 365;

    /// <summary>
    /// Fails fast on a misconfigured or missing signing setup — an API that starts
    /// with a weak or absent JWT secret is worse than one that refuses to start.
    /// </summary>
    /// <exception cref="InvalidOperationException">Any required value is missing or too weak.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Secret))
        {
            throw new InvalidOperationException(
                "Jwt:Secret is not configured. Set it with: dotnet user-secrets set \"Jwt:Secret\" \"<at least 32 characters>\"");
        }

        if (Secret.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must be at least 32 characters long to sign with HMAC-SHA256.");
        }

        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must both be configured.");
        }

        if (EmployeeExpireHours <= 0)
        {
            throw new InvalidOperationException("Jwt:EmployeeExpireHours must be a positive number of hours.");
        }

        if (CustomerExpireDays <= 0)
        {
            throw new InvalidOperationException("Jwt:CustomerExpireDays must be a positive number of days.");
        }
    }
}
