using CoffeeLoyalty.Api.Dtos.Auth;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// The two ways into this API: staff sign-in with a password, and customer
/// token exchange against Firebase.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates an employee and issues a JWT.
    /// </summary>
    /// <param name="request">Username and password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The token plus the display fields the dashboard needs.</returns>
    /// <exception cref="Common.ApiException">
    /// 401 <c>INVALID_CREDENTIALS</c> for an unknown username or wrong password;
    /// 401 <c>ACCOUNT_DISABLED</c> for a deactivated employee (decision 18);
    /// 429 <c>TOO_MANY_REQUESTS</c> once this username has failed too often (decision 27).
    /// </exception>
    Task<EmployeeLoginResponse> LoginEmployeeAsync(EmployeeLoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a verified Firebase ID token for our own JWT, creating the
    /// customer on first login (decision 5) and linking the Firebase uid by
    /// normalized phone number (decision 11).
    /// </summary>
    /// <param name="request">The Firebase ID token, plus a full name used only when creating.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The token plus the customer's name and balance.</returns>
    /// <exception cref="Common.ApiException">
    /// 401 <c>INVALID_FIREBASE_TOKEN</c> when verification fails;
    /// 400 <c>INVALID_PHONE</c> when the verified number is not a Jordanian mobile;
    /// 429 <c>TOO_MANY_REQUESTS</c> once this phone number has failed too often (decision 27).
    /// </exception>
    Task<CustomerLoginResponse> LoginCustomerAsync(FirebaseLoginRequest request, CancellationToken cancellationToken = default);
}
