using System.ComponentModel.DataAnnotations;

namespace CoffeeLoyalty.Api.Dtos.Employees;

/// <summary>
/// Body of <c>PATCH /api/employees/{id}/status</c> — activate or deactivate an account
/// (decision 18). Never a physical delete: the row is kept so that the orders that
/// reference the employee keep resolving.
/// </summary>
public class UpdateEmployeeStatusRequest
{
    /// <summary>
    /// The new state. Nullable so that an omitted field is a <c>VALIDATION_ERROR</c>
    /// rather than a silent <c>false</c> that locks a colleague out of their account.
    /// </summary>
    [Required]
    public bool? IsActive { get; set; }
}
