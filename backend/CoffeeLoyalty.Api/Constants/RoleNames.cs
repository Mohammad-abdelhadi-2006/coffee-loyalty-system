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

    /// <summary>
    /// The inverse of <see cref="ToWireString"/>: a wire string back to the stored enum.
    /// </summary>
    /// <param name="role">The role as it arrived in the request body.</param>
    /// <param name="parsed">The matching <see cref="EmployeeRole"/>, or <c>default</c> when there is none.</param>
    /// <returns><c>true</c> when the string is one of the two roles.</returns>
    /// <remarks>
    /// Written by hand rather than delegated to <see cref="Enum.TryParse{TEnum}(string, bool, out TEnum)"/>
    /// or to the JSON enum converter, because the wire form is lowercase and the enum
    /// members are not: the two vocabularies happen to differ only in case today, and
    /// nothing should let a renamed member silently redefine what clients may send.
    /// Matching is case-insensitive, so <c>"Cashier"</c> is accepted as well —
    /// rejecting a client over capitalization buys nothing.
    /// </remarks>
    public static bool TryParseWireString(string? role, out EmployeeRole parsed)
    {
        if (string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase))
        {
            parsed = EmployeeRole.Admin;
            return true;
        }

        if (string.Equals(role, Cashier, StringComparison.OrdinalIgnoreCase))
        {
            parsed = EmployeeRole.Cashier;
            return true;
        }

        parsed = default;
        return false;
    }
}
