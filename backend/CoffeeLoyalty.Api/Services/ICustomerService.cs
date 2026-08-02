using CoffeeLoyalty.Api.Dtos.Customers;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// Customer records (api-contract.md §4): the counter's registration and lookup, and the
/// app's view of its own account.
/// </summary>
/// <remarks>
/// Reading a customer's orders and points history is deliberately absent — those shapes
/// belong to the orders module and are built with it.
/// </remarks>
public interface ICustomerService
{
    /// <summary>
    /// Registers a customer at the counter, with no Firebase account yet: the uid is
    /// linked by phone number the first time they sign in to the app (decision 11).
    /// </summary>
    /// <param name="request">Display name and phone number in any accepted spelling.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created customer, with a zero balance.</returns>
    /// <exception cref="Common.ApiException">
    /// 400 <c>INVALID_PHONE</c> when the number is not a Jordanian mobile;
    /// 409 <c>PHONE_ALREADY_EXISTS</c> when it already identifies someone;
    /// 400 <c>VALIDATION_ERROR</c> for a blank name.
    /// </exception>
    Task<CustomerResponse> RegisterAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a customer by phone number — the cashier's main search (decision 7).
    /// </summary>
    /// <param name="phoneNumber">The number as typed; normalized before the lookup.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching customer.</returns>
    /// <exception cref="Common.ApiException">
    /// 400 <c>INVALID_PHONE</c> when the number is not a Jordanian mobile;
    /// 404 <c>CUSTOMER_NOT_FOUND</c> when nobody is registered under it.
    /// </exception>
    Task<CustomerResponse> FindByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// The signed-in customer's own account.
    /// </summary>
    /// <param name="customerId">The id from the token's <c>sub</c> claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Name, phone number and balance — nothing internal.</returns>
    /// <exception cref="Common.ApiException">404 <c>CUSTOMER_NOT_FOUND</c> when the row behind the token is gone.</exception>
    Task<CustomerProfileResponse> GetProfileAsync(int customerId, CancellationToken cancellationToken = default);
}
