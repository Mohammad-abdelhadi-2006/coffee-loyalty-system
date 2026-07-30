using System.ComponentModel.DataAnnotations;

namespace CoffeeLoyalty.Api.Dtos.Auth;

/// <summary>
/// Body of <c>POST /api/auth/firebase-login</c> — the app exchanges a Firebase
/// ID token (obtained via OTP) for our own JWT (decision 5).
/// </summary>
public class FirebaseLoginRequest
{
    /// <summary>The Firebase ID token produced by the client after OTP verification.</summary>
    [Required]
    public string FirebaseIdToken { get; set; } = string.Empty;

    /// <summary>
    /// Display name, used <b>only</b> when this login creates a new customer.
    /// Ignored when the phone number already belongs to a customer.
    /// </summary>
    [MaxLength(100)]
    public string? FullName { get; set; }
}
