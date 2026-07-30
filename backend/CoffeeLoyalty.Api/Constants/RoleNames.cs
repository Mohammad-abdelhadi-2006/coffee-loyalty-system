using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Constants;

/// <summary>
/// The canonical lowercase role strings used on the wire — both the JWT <c>role</c>
/// claim and the JSON <c>role</c> field (api-contract.md → Conventions → Auth levels).
/// These double as the authorization policy names registered in Program.cs.
/// </summary>
public static class RoleNames
{
    /// <summary>App user authenticated through Firebase OTP.</summary>
    public const string Customer = "customer";

    /// <summary>Counter staff. The <c>cashier</c> policy also admits <see cref="Admin"/>.</summary>
    public const string Cashier = "cashier";

    /// <summary>Shop owner / manager.</summary>
    public const string Admin = "admin";

    /// <summary>
    /// Maps a stored <see cref="EmployeeRole"/> to its canonical wire string.
    /// </summary>
    /// <param name="role">The role as persisted on the Employee row.</param>
    /// <returns><c>"admin"</c> or <c>"cashier"</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The enum gained a member with no wire mapping.</exception>
    public static string ToWireString(EmployeeRole role) => role switch
    {
        EmployeeRole.Admin => Admin,
        EmployeeRole.Cashier => Cashier,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unmapped employee role.")
    };
}
