using CoffeeLoyalty.Api.Dtos.Orders;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// Orders and their points movements (api-contract.md §5). This is the only place in the
/// system that mints points, so it is also the only place that has to keep
/// <c>Customer.PointsBalance</c> equal to the sum of that customer's transaction rows.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Records a sale: the order, its lines, the points it earns, the points it spends,
    /// and the resulting balance.
    /// </summary>
    /// <param name="employeeId">The cashier ringing it up, from their token's <c>sub</c> claim.</param>
    /// <param name="request">The customer, the points to spend, and the basket.</param>
    /// <param name="idempotencyKey">
    /// The client's <c>Idempotency-Key</c> header, or null when it sent none. A key already
    /// carried by an order makes this call a replay: it returns that order's receipt and rings
    /// nothing up (decision 40). Without a key the call behaves exactly as it always has.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The receipt figures, including the customer's new balance.</returns>
    /// <remarks>
    /// Everything it writes — the Order, every OrderItem, the <c>Redeem</c> and <c>Earn</c>
    /// transactions and the balance update — commits as one database transaction or not at
    /// all. A half-written order would leave the balance disagreeing with the ledger, which
    /// no later request could detect, let alone repair (decision 11).
    /// </remarks>
    /// <exception cref="Common.ApiException">
    /// 404 <c>CUSTOMER_NOT_FOUND</c> / <c>PRODUCT_NOT_FOUND</c>;
    /// 400 <c>PRODUCT_UNAVAILABLE</c>, <c>INVALID_QUANTITY</c>, <c>REDEEM_BELOW_MINIMUM</c>,
    /// <c>INSUFFICIENT_BALANCE</c>, <c>REDEEM_EXCEEDS_TOTAL</c>, or <c>VALIDATION_ERROR</c>
    /// for an over-long idempotency key.
    /// </exception>
    Task<CreateOrderResponse> CreateAsync(
        int employeeId,
        CreateOrderRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads one order in full, including who bought it and who rang it up.
    /// </summary>
    /// <param name="orderId">The order's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order, its lines, and the two names.</returns>
    /// <exception cref="Common.ApiException">404 <c>ORDER_NOT_FOUND</c>.</exception>
    Task<OrderDetailResponse> GetByIdAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The customer's most recent orders — what the cashier picks from on the returns screen.
    /// </summary>
    /// <param name="customerId">The customer's id.</param>
    /// <param name="limit">How many to return, newest first.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Up to <paramref name="limit"/> orders, newest first; empty when the customer has never bought anything.</returns>
    /// <remarks>
    /// Cancelled orders are included. The screen has to show them — otherwise a cashier who
    /// cannot find an order they know exists will simply ring the refund up a second way.
    /// </remarks>
    /// <exception cref="Common.ApiException">404 <c>CUSTOMER_NOT_FOUND</c>.</exception>
    Task<IReadOnlyList<OrderResponse>> GetForCustomerAsync(
        int customerId,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an order outright: claws back what it earned and hands back what it spent
    /// (decision 6).
    /// </summary>
    /// <param name="orderId">The order's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>What moved, and the resulting balance.</returns>
    /// <remarks>
    /// Only reachable from a <c>Completed</c> order with no returns. An order that has had
    /// even one item returned takes the line-by-line path instead, because reversing it
    /// wholesale on top of a partial <c>Refund</c> would reverse the same points twice
    /// (decision 20). Everything commits as one database transaction.
    /// <para>
    /// Writes on one order are serialized (decision 39): a second cancel arriving while this
    /// one is open waits for it to commit and is then rejected by the guards above, rather
    /// than reading the same <c>Completed</c> status and clawing the points back twice.
    /// </para>
    /// </remarks>
    /// <exception cref="Common.ApiException">
    /// 404 <c>ORDER_NOT_FOUND</c>; 400 <c>ORDER_ALREADY_CANCELLED</c>, <c>ORDER_HAS_RETURNS</c>,
    /// <c>RETURN_WINDOW_EXPIRED</c>, <c>INSUFFICIENT_BALANCE_FOR_RETURN</c>.
    /// </exception>
    Task<CancelOrderResponse> CancelAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns part of an order, line by line, clawing back the points that value earned.
    /// </summary>
    /// <param name="orderId">The order's id.</param>
    /// <param name="request">The lines coming back and how much of each.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cash owed for this return, the points it took back, and the new balance.</returns>
    /// <remarks>
    /// Only orders that spent no points can be returned this way (decision 19); one paid
    /// with points has to be cancelled outright, because a fraction of a redemption cannot
    /// be handed back coherently.
    /// <para>
    /// The claw-back is cumulative rather than per-return: each call computes what the
    /// order's <i>whole</i> returned value should have cost so far and subtracts what has
    /// already been taken. Rounding therefore cannot accumulate — returning an order line by
    /// line claws back exactly <c>PointsEarned</c>, no matter how the lines are grouped or
    /// in what order they come back. Everything commits as one database transaction.
    /// </para>
    /// <para>
    /// Writes on one order are serialized (decision 39): two returns submitted at once are
    /// applied one after the other, so the second computes its claw-back against the
    /// quantities the first actually committed.
    /// </para>
    /// </remarks>
    /// <exception cref="Common.ApiException">
    /// 404 <c>ORDER_NOT_FOUND</c>; 400 <c>ORDER_ALREADY_CANCELLED</c>, <c>ORDER_PAID_WITH_POINTS</c>,
    /// <c>ITEM_NOT_IN_ORDER</c>, <c>RETURN_EXCEEDS_QUANTITY</c>, <c>RETURN_WINDOW_EXPIRED</c>,
    /// <c>INSUFFICIENT_BALANCE_FOR_RETURN</c>.
    /// </exception>
    Task<ReturnOrderResponse> ReturnItemsAsync(
        int orderId,
        ReturnOrderRequest request,
        CancellationToken cancellationToken = default);
}
