using System.ComponentModel.DataAnnotations;

namespace CoffeeLoyalty.Api.Dtos.Employees;

/// <summary>
/// Body of <c>POST /api/employees</c> — the admin creating a staff account. There is no
/// self-registration anywhere in this system (decision 4), so this is the only way an
/// account other than the seeded admin comes into existence.
/// </summary>
public class CreateEmployeeRequest
{
    /// <summary>The employee's display name. Capped at the column width on Employee.</summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>The login name. Must not already be taken.</summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The initial plaintext password, stored only as a BCrypt hash. The 72-character cap
    /// is BCrypt's own: it ignores everything past the first 72 bytes, so a longer password
    /// would be accepted here and then silently truncated when it is hashed — the same cap
    /// the login DTO carries, so what can be set is exactly what can be submitted.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MinLength(8)]
    [MaxLength(72)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// <c>"cashier"</c> or <c>"admin"</c> — the lowercase wire form, mapped explicitly to
    /// the stored enum. Declared as a string so that an unknown value is rejected by that
    /// mapping with a message about roles, rather than by the JSON reader as a malformed body.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;
}
