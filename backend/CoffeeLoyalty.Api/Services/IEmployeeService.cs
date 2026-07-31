using CoffeeLoyalty.Api.Dtos.Employees;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// Staff accounts (api-contract.md §2). Admin-only throughout: this is how the shop
/// owner creates the counter's logins and closes them again.
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Lists every employee, active and deactivated alike — the admin needs to see a
    /// closed account in order to reopen it.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The accounts, oldest first.</returns>
    Task<IReadOnlyList<EmployeeResponse>> GetEmployeesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a staff account with a BCrypt-hashed password (decision 4).
    /// </summary>
    /// <param name="request">Display name, login name, initial password and role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created employee, without the password.</returns>
    /// <exception cref="Common.ApiException">
    /// 400 <c>VALIDATION_ERROR</c> when the username is taken, the role is not one of the
    /// two wire strings, or a name is blank.
    /// </exception>
    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates or deactivates an account (decision 18).
    /// </summary>
    /// <param name="id">The employee being changed.</param>
    /// <param name="request">The new state.</param>
    /// <param name="currentEmployeeId">The admin making the call, from their token's <c>sub</c> claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated employee.</returns>
    /// <remarks>
    /// Deactivation also revokes the account's tokens on the spot by bumping its
    /// <c>TokenVersion</c> (decision 25), so a cashier who is let go cannot keep working
    /// through the rest of their token's lifetime.
    /// </remarks>
    /// <exception cref="Common.ApiException">
    /// 404 <c>EMPLOYEE_NOT_FOUND</c> when no such employee exists;
    /// 400 <c>VALIDATION_ERROR</c> when an admin tries to deactivate their own account.
    /// </exception>
    Task<EmployeeResponse> SetStatusAsync(
        int id,
        UpdateEmployeeStatusRequest request,
        int currentEmployeeId,
        CancellationToken cancellationToken = default);
}
