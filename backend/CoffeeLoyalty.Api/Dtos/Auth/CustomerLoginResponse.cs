namespace CoffeeLoyalty.Api.Dtos.Auth;

/// <summary>
/// Success body of <c>POST /api/auth/firebase-login</c>.
/// </summary>
public class CustomerLoginResponse
{
    /// <summary>The signed JWT with <c>role=customer</c>.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>The customer's display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>The current points balance, so the app can render the home screen immediately.</summary>
    public int PointsBalance { get; set; }

    /// <summary>When the token stops being accepted (UTC).</summary>
    public DateTimeOffset ExpiresAt { get; set; }
}
