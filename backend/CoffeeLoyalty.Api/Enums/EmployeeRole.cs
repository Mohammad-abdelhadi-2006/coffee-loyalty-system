namespace CoffeeLoyalty.Api.Enums;

/// <summary>
/// Staff roles. Stored as a string via HasConversion&lt;string&gt;(), so the member
/// name is the database value — renaming a member is a data change, not a schema change.
/// The wire format (JWT claim + JSON) is the lowercase form in <see cref="Constants.RoleNames"/>.
/// </summary>
public enum EmployeeRole
{
    /// <summary>Full access; can do everything a cashier can, plus management.</summary>
    Admin,

    /// <summary>Counter staff: orders, returns, customer registration.</summary>
    Cashier
}
