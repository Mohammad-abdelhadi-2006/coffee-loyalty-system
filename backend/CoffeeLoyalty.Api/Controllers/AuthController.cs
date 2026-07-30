using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Dtos.Auth;
using CoffeeLoyalty.Api.Extensions;
using CoffeeLoyalty.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoffeeLoyalty.Api.Controllers;

/// <summary>
/// The two public entry points of the API (api-contract.md §1). Both are thin —
/// every rule lives in <see cref="IAuthService"/>.
/// </summary>
/// <remarks>
/// These are the only anonymous endpoints that authenticate anybody, so they are the
/// only ones rate-limited (decision 26): both are worth brute-forcing and neither is
/// on a hot path.
/// </remarks>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[EnableRateLimiting(AuthSetupExtensions.AuthRateLimitPolicy)]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    /// <summary>
    /// Creates the controller.
    /// </summary>
    /// <param name="auth">The authentication service.</param>
    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>
    /// Employee login for the dashboard.
    /// </summary>
    /// <param name="request">Username and password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JWT with the employee's role, name, and expiry.</returns>
    /// <response code="200">Authenticated; the token is ready to use.</response>
    /// <response code="400">VALIDATION_ERROR — malformed body.</response>
    /// <response code="401">INVALID_CREDENTIALS or ACCOUNT_DISABLED.</response>
    /// <response code="429">TOO_MANY_REQUESTS — too many attempts from this client IP.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(EmployeeLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmployeeLoginResponse>> Login(
        [FromBody] EmployeeLoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _auth.LoginEmployeeAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Customer token exchange: a verified Firebase ID token in, our own JWT out.
    /// </summary>
    /// <param name="request">The Firebase ID token, plus a name used only on first login.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JWT with role <c>customer</c>, the name, and the points balance.</returns>
    /// <response code="200">Authenticated; the customer was found or created.</response>
    /// <response code="400">INVALID_PHONE or VALIDATION_ERROR.</response>
    /// <response code="401">INVALID_FIREBASE_TOKEN.</response>
    /// <response code="429">TOO_MANY_REQUESTS — too many attempts from this client IP.</response>
    [HttpPost("firebase-login")]
    [ProducesResponseType(typeof(CustomerLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CustomerLoginResponse>> FirebaseLogin(
        [FromBody] FirebaseLoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _auth.LoginCustomerAsync(request, cancellationToken);
        return Ok(response);
    }
}
