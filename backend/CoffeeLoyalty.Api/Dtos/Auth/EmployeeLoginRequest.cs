using System.ComponentModel.DataAnnotations;

namespace CoffeeLoyalty.Api.Dtos.Auth;

/// <summary>
/// Body of <c>POST /api/auth/login</c> — the dashboard's employee sign-in.
/// </summary>
public class EmployeeLoginRequest
{
    /// <summary>The employee's login name (unique on Employee).</summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The plaintext password, verified against the stored BCrypt hash. Capped at 72
    /// because BCrypt silently ignores everything past the first 72 bytes of input —
    /// without the cap, a longer password would validate here and then be truncated
    /// during verification, so the accepted length would not be the checked length.
    /// (The cap counts characters; non-ASCII passwords hit BCrypt's byte limit sooner.)
    /// </summary>
    [Required]
    [MaxLength(72)]
    public string Password { get; set; } = string.Empty;
}
