using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Constants;
using CoffeeLoyalty.Api.Dtos.Orders;
using CoffeeLoyalty.Api.Extensions;
using CoffeeLoyalty.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeLoyalty.Api.Controllers;

/// <summary>
/// Sales and their points movements (api-contract.md §5). Thin — every rule lives in
/// <see cref="IOrderService"/>.
/// </summary>
/// <remarks>
/// The whole controller is the cashier's: an order is rung up at the counter, and the
/// policy admits admins as well, per the contract's auth levels.
/// </remarks>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Authorize(Policy = RoleNames.Cashier)]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    /// <summary>
    /// Creates the controller.
    /// </summary>
    /// <param name="orders">The orders service.</param>
    public OrdersController(IOrderService orders)
    {
        _orders = orders;
    }

    /// <summary>
    /// Rings up a sale.
    /// </summary>
    /// <param name="request">The customer, the points to spend, and the basket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The receipt figures and the customer's new balance.</returns>
    /// <remarks>
    /// The cashier is taken from the token's <c>sub</c> claim, never from the body: who
    /// rang an order up is a fact about the session, and a client that could name someone
    /// else would be able to file its sales under another employee.
    /// </remarks>
    /// <response code="201">Created; the body is the receipt.</response>
    /// <response code="400">PRODUCT_UNAVAILABLE, INVALID_QUANTITY, REDEEM_BELOW_MINIMUM, INSUFFICIENT_BALANCE, REDEEM_EXCEEDS_TOTAL or VALIDATION_ERROR.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a cashier or admin.</response>
    /// <response code="404">CUSTOMER_NOT_FOUND or PRODUCT_NOT_FOUND.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _orders.CreateAsync(User.GetUserId(), request, cancellationToken);

        return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
    }

    /// <summary>
    /// Reads one order in full.
    /// </summary>
    /// <param name="id">The order's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order, its lines, and the customer's and cashier's names.</returns>
    /// <response code="200">The order.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a cashier or admin.</response>
    /// <response code="404">ORDER_NOT_FOUND.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailResponse>> GetOrder(int id, CancellationToken cancellationToken)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken);
        return Ok(order);
    }

    /// <summary>
    /// Cancels an order in full (decision 6).
    /// </summary>
    /// <param name="id">The order's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The points moved and the customer's new balance.</returns>
    /// <remarks>
    /// The contract gives this an empty body — there is nothing to choose. Cancellation is
    /// all-or-nothing by definition; anything selective is a return.
    /// </remarks>
    /// <response code="200">Cancelled.</response>
    /// <response code="400">ORDER_ALREADY_CANCELLED, ORDER_HAS_RETURNS, RETURN_WINDOW_EXPIRED or INSUFFICIENT_BALANCE_FOR_RETURN.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a cashier or admin.</response>
    /// <response code="404">ORDER_NOT_FOUND.</response>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(CancelOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CancelOrderResponse>> CancelOrder(int id, CancellationToken cancellationToken)
    {
        var result = await _orders.CancelAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns part of an order, line by line (decision 19).
    /// </summary>
    /// <param name="id">The order's id.</param>
    /// <param name="request">The lines coming back and how much of each.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cash owed, the points clawed back, and the customer's new balance.</returns>
    /// <response code="200">Returned.</response>
    /// <response code="400">ORDER_ALREADY_CANCELLED, ORDER_PAID_WITH_POINTS, ITEM_NOT_IN_ORDER, RETURN_EXCEEDS_QUANTITY, RETURN_WINDOW_EXPIRED, INSUFFICIENT_BALANCE_FOR_RETURN or VALIDATION_ERROR.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a cashier or admin.</response>
    /// <response code="404">ORDER_NOT_FOUND.</response>
    [HttpPost("{id:int}/returns")]
    [ProducesResponseType(typeof(ReturnOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReturnOrderResponse>> ReturnItems(
        int id,
        [FromBody] ReturnOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orders.ReturnItemsAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
