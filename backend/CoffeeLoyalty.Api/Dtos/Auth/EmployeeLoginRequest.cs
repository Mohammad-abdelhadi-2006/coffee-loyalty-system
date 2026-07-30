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

    /// <summary>The plaintext password, verified against the stored BCrypt hash.</summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}
