using System.Security.Claims;
using System.Text;
using CoffeeLoyalty.Api.Constants;
using CoffeeLoyalty.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// HMAC-SHA256 token issuer backed by the <c>Jwt</c> configuration section.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    /// <summary>The claim type carrying the role, matching TokenValidationParameters.RoleClaimType.</summary>
    public const string RoleClaimType = "role";

    /// <summary>The claim type carrying the display name, matching TokenValidationParameters.NameClaimType.</summary>
    public const string NameClaimType = "name";

    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    /// <summary>
    /// Creates the issuer and derives the signing key once.
    /// </summary>
    /// <param name="options">The validated <c>Jwt</c> section.</param>
    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    /// <inheritdoc />
    public string CreateToken(int userId, string role, string fullName, out DateTimeOffset expiresAt)
    {
        // JWT `exp` has one-second resolution; truncate so the value we hand the
        // client is exactly the moment the token dies, not a fraction after it.
        var now = DateTimeOffset.UtcNow.UtcDateTime;
        var issuedAt = new DateTimeOffset(now.AddTicks(-(now.Ticks % TimeSpan.TicksPerSecond)), TimeSpan.Zero);

        // Decision 15: a customer re-authenticating means another OTP, so their token
        // is long-lived; staff tokens last a shift.
        expiresAt = role == RoleNames.Customer
            ? issuedAt.AddDays(_options.CustomerExpireDays)
            : issuedAt.AddHours(_options.EmployeeExpireHours);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = _signingCredentials,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(RoleClaimType, role),
                new Claim(NameClaimType, fullName)
            ])
        };

        // JsonWebTokenHandler writes claim types verbatim — no outbound URI mapping,
        // so `role` and `name` stay short exactly as the contract specifies.
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
