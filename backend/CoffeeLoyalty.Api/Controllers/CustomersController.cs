using System.ComponentModel.DataAnnotations;
using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Constants;
using CoffeeLoyalty.Api.Dtos.Customers;
using CoffeeLoyalty.Api.Dtos.Orders;
using CoffeeLoyalty.Api.Extensions;
using CoffeeLoyalty.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeLoyalty.Api.Controllers;

/// <summary>
/// Customer records (api-contract.md §4). Thin — every rule lives in
/// <see cref="ICustomerService"/>.
/// </summary>
/// <remarks>
/// Two of these routes read order and points history rather than the customer record, so
/// the controller talks to <see cref="IOrderService"/> as well: the order list is an
/// orders-module shape that happens to be addressed by customer, while the points ledger
/// applies no rule of its own and stays with the customer's own account.
/// </remarks>
[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;
    private readonly IOrderService _orders;

    /// <summary>
    /// Creates the controller.
    /// </summary>
    /// <param name="customers">The customer service.</param>
    /// <param name="orders">The orders service, for the order-history route.</param>
    public CustomersController(ICustomerService customers, IOrderService orders)
    {
        _customers = customers;
        _orders = orders;
    }

    /// <summary>
    /// Registers a customer at the counter.
    /// </summary>
    /// <param name="request">Display name and phone number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created customer.</returns>
    /// <response code="201">Created; the body is the new customer, balance zero.</response>
    /// <response code="400">INVALID_PHONE or VALIDATION_ERROR.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a cashier or admin.</response>
    /// <response code="409">PHONE_ALREADY_EXISTS.</response>
    [HttpPost]
    [Authorize(Policy = RoleNames.Cashier)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> RegisterCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.RegisterAsync(request, cancellationToken);

        // 201 without a Location header: the contract defines no GET /api/customers/{id},
        // so there is no URI to point the client at.
        return StatusCode(StatusCodes.Status201Created, customer);
    }

    /// <summary>
    /// Looks a customer up by phone number — the cashier's main search (decision 7).
    /// </summary>
    /// <param name="phone">The number in any accepted spelling; normalized before the lookup.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching customer.</returns>
    /// <remarks>
    /// The query string is required, so an omitted <c>?phone=</c> is a malformed request
    /// (<c>VALIDATION_ERROR</c>) rather than a phone number that happens to be invalid —
    /// the client is missing a parameter, not holding a bad number.
    /// </remarks>
    /// <response code="200">The customer.</response>
    /// <response code="400">INVALID_PHONE, or VALIDATION_ERROR when the parameter is missing.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a cashier or admin.</response>
    /// <response code="404">CUSTOMER_NOT_FOUND.</response>
    [HttpGet]
    [Authorize(Policy = RoleNames.Cashier)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> FindCustomer(
        [FromQuery][Required(AllowEmptyStrings = false)] string phone,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.FindByPhoneAsync(phone, cancellationToken);
        return Ok(customer);
    }

    /// <summary>
    /// The signed-in customer's own account.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Name, phone number and points balance.</returns>
    /// <remarks>
    /// The id comes from the token's <c>sub</c> claim and from nowhere else — there is no
    /// route or query parameter here that could be pointed at somebody else's account.
    /// </remarks>
    /// <response code="200">The caller's profile.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a customer token.</response>
    /// <response code="404">CUSTOMER_NOT_FOUND — the row behind the token is gone.</response>
    [HttpGet("me")]
    [Authorize(Policy = RoleNames.Customer)]
    [ProducesResponseType(typeof(CustomerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerProfileResponse>> GetMe(CancellationToken cancellationToken)
    {
        var profile = await _customers.GetProfileAsync(User.GetUserId(), cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// A customer's recent orders — the returns screen's list.
    /// </summary>
    /// <param name="id">The customer's id.</param>
    /// <param name="limit">How many orders to return, newest first. Defaults to 10, capped at 50.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The orders with their lines.</returns>
    /// <remarks>
    /// The cap is a <see cref="RangeAttribute"/> rather than a silent clamp: a client asking
    /// for 500 has misunderstood the endpoint, and quietly handing back 50 would let it
    /// believe it had seen the customer's whole history.
    /// </remarks>
    /// <response code="200">The orders; empty when the customer has never bought anything.</response>
    /// <response code="400">VALIDATION_ERROR — limit outside 1..50.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a cashier or admin.</response>
    /// <response code="404">CUSTOMER_NOT_FOUND.</response>
    [HttpGet("{id:int}/orders")]
    [Authorize(Policy = RoleNames.Cashier)]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetCustomerOrders(
        int id,
        [FromQuery][Range(1, 50)] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orders.GetForCustomerAsync(id, limit, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// The signed-in customer's own points ledger.
    /// </summary>
    /// <param name="limit">How many movements to return, newest first. Defaults to 20, capped at 100.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The movements behind the caller's balance.</returns>
    /// <remarks>
    /// Like <c>/me</c>, the id comes from the token's <c>sub</c> claim and from nowhere
    /// else — there is no parameter here that could be pointed at another customer's ledger.
    /// </remarks>
    /// <response code="200">The ledger; empty for a customer with no movements yet.</response>
    /// <response code="400">VALIDATION_ERROR — limit outside 1..100.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a customer token.</response>
    [HttpGet("me/transactions")]
    [Authorize(Policy = RoleNames.Customer)]
    [ProducesResponseType(typeof(IReadOnlyList<PointsTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PointsTransactionResponse>>> GetMyTransactions(
        [FromQuery][Range(1, 100)] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _customers.GetTransactionsAsync(User.GetUserId(), limit, cancellationToken);
        return Ok(transactions);
    }

    /// <summary>
    /// The signed-in customer's own purchase history — the mobile app's "My Purchases" list.
    /// </summary>
    /// <param name="limit">How many orders to return, newest first. Defaults to 10, capped at 50.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The caller's orders with their lines.</returns>
    /// <remarks>
    /// Like <c>/me/transactions</c>, the id comes from the token's <c>sub</c> claim and from
    /// nowhere else — there is no parameter here that could be pointed at another customer's
    /// orders. That is also why no 404 is documented: <see cref="IOrderService.GetForCustomerAsync"/>
    /// raises <c>CUSTOMER_NOT_FOUND</c> for an unknown id, but a customer token is only issued
    /// for a customer that exists, so the code is unreachable on this route.
    /// The list shape carries no cashier or customer name, so the app cannot read who rang the
    /// order up.
    /// </remarks>
    /// <response code="200">The orders; empty for a customer who has never bought anything.</response>
    /// <response code="400">VALIDATION_ERROR — limit outside 1..50.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a customer token.</response>
    [HttpGet("me/orders")]
    [Authorize(Policy = RoleNames.Customer)]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetMyOrders(
        [FromQuery][Range(1, 50)] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orders.GetForCustomerAsync(User.GetUserId(), limit, cancellationToken);
        return Ok(orders);
    }
}
