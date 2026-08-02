using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Constants;
using CoffeeLoyalty.Api.Dtos.Employees;
using CoffeeLoyalty.Api.Extensions;
using CoffeeLoyalty.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeLoyalty.Api.Controllers;

/// <summary>
/// Staff accounts (api-contract.md §2). Thin — every rule lives in
/// <see cref="IEmployeeService"/>.
/// </summary>
/// <remarks>
/// Admin-only end to end, so the policy sits on the controller rather than on each action.
/// </remarks>
[ApiController]
[Route("api/employees")]
[Authorize(Policy = RoleNames.Admin)]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employees;

    /// <summary>
    /// Creates the controller.
    /// </summary>
    /// <param name="employees">The employee service.</param>
    public EmployeesController(IEmployeeService employees)
    {
        _employees = employees;
    }

    /// <summary>
    /// Lists the staff accounts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Every employee, deactivated ones included.</returns>
    /// <response code="200">The accounts.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not an admin.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<EmployeeResponse>>> GetEmployees(CancellationToken cancellationToken)
    {
        var employees = await _employees.GetEmployeesAsync(cancellationToken);
        return Ok(employees);
    }

    /// <summary>
    /// Creates a staff account (decision 4 — there is no self-registration).
    /// </summary>
    /// <param name="request">Display name, login name, initial password and role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created employee, without the password.</returns>
    /// <response code="201">Created; the body is the new employee.</response>
    /// <response code="400">VALIDATION_ERROR — malformed body, unknown role, or the username is taken.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not an admin.</response>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployeeResponse>> CreateEmployee(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await _employees.CreateAsync(request, cancellationToken);

        // 201 without a Location header: the contract defines no GET /api/employees/{id},
        // so there is no URI to point the client at.
        return StatusCode(StatusCodes.Status201Created, employee);
    }

    /// <summary>
    /// Activates or deactivates an account (decision 18). Never a physical delete.
    /// </summary>
    /// <param name="id">The employee being changed.</param>
    /// <param name="request">The new state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated employee.</returns>
    /// <remarks>
    /// The caller's own id comes from their token, so "am I deactivating myself?" is
    /// decided by who is signed in and not by anything the request could claim to be.
    /// </remarks>
    /// <response code="200">Updated. A deactivation has already revoked that account's tokens.</response>
    /// <response code="400">VALIDATION_ERROR — malformed body, or self-deactivation.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not an admin.</response>
    /// <response code="404">EMPLOYEE_NOT_FOUND.</response>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> SetStatus(
        int id,
        [FromBody] UpdateEmployeeStatusRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await _employees.SetStatusAsync(id, request, User.GetUserId(), cancellationToken);
        return Ok(employee);
    }
}
