namespace CoffeeLoyalty.Api.Dtos.Auth;

/// <summary>
/// Success body of <c>POST /api/auth/login</c>.
/// </summary>
public class EmployeeLoginResponse
{
    /// <summary>The signed JWT, sent back as <c>Authorization: Bearer &lt;jwt&gt;</c>.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>The employee's display name, so the dashboard need not decode the token.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>The canonical role string: <c>admin</c> or <c>cashier</c>.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>When the token stops being accepted (UTC).</summary>
    public DateTimeOffset ExpiresAt { get; set; }
}
