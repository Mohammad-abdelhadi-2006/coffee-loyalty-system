namespace CoffeeLoyalty.Api.Dtos.Employees;

/// <summary>
/// The employee object every route in api-contract.md §2 returns — the list, the create
/// and the status toggle all speak this.
/// </summary>
/// <remarks>
/// <see cref="Role"/> is a <see cref="string"/> and not the <c>EmployeeRole</c> enum: the
/// wire vocabulary is the lowercase one in <c>RoleNames</c>, which does not match the enum
/// member names, so this field is mapped explicitly and is untouched by the global
/// <c>JsonStringEnumConverter</c>. <c>PasswordHash</c> and <c>TokenVersion</c> are
/// internal and are exposed by no endpoint.
/// </remarks>
public class EmployeeResponse
{
    /// <summary>Primary key; the id the status route takes.</summary>
    public int Id { get; set; }

    /// <summary>The employee's display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>The login name (unique).</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>The canonical role string: <c>admin</c> or <c>cashier</c>.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Whether the account can still sign in (decision 18).</summary>
    public bool IsActive { get; set; }

    /// <summary>When the account was created (UTC).</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
