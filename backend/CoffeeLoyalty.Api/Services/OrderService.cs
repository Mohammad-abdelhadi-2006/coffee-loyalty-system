using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Constants;
using CoffeeLoyalty.Api.Data;
using CoffeeLoyalty.Api.Dtos.Orders;
using CoffeeLoyalty.Api.Entities;
using CoffeeLoyalty.Api.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// All order and points rules live here; the controller only maps HTTP to these calls.
/// </summary>
public sealed class OrderService : IOrderService
{
    /// <summary>
    /// Decimal places the money columns keep — <c>decimal(18,3)</c> on both
    /// <c>Order.Total</c> and <c>OrderItem.UnitPriceSnapshot</c>. JOD is quoted in fils,
    /// so three decimals is the whole currency, not a truncation of it.
    /// </summary>
    private const int MoneyScale = 3;

    private readonly AppDbContext _db;
    private readonly ILogger<OrderService> _logger;

    /// <summary>
    /// Creates the service.
    /// </summary>
    /// <param name="db">The application database context.</param>
    /// <param name="logger">Diagnostics; never surfaced to the caller.</param>
    public OrderService(AppDbContext db, ILogger<OrderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CreateOrderResponse> CreateAsync(
        int employeeId,
        CreateOrderRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        // Non-null by the time the action body runs: [Required] on the nullable ids means
        // model validation has already rejected an absent one.
        var customerId = request.CustomerId!.Value;
        var pointsRedeemed = request.PointsRedeemed ?? 0;
        var key = NormalizeIdempotencyKey(idempotencyKey);

        // One transaction around the reads as well as the writes: the prices that are
        // snapshotted and the availability that is checked have to be the same rows the
        // order is finally written against.
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        if (key is not null)
        {
            // The ordinary replay: the first attempt already committed, so the key is on a
            // row and the retry never rings anything up. The *race* — a retry that arrives
            // while the first attempt is still open — cannot be seen here and is caught on
            // the insert below (decision 40). Returning without committing is deliberate:
            // nothing was written, so disposal's rollback is the whole cleanup.
            var replay = await FindByIdempotencyKeyAsync(key, cancellationToken);

            if (replay is not null)
            {
                return replay;
            }
        }

        if (!await _db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken))
        {
            throw ApiException.CustomerNotFound();
        }

        var items = await BuildItemsAsync(request.Items, cancellationToken);

        // Σ of the line values. Each line is rounded to fils on its own so that the stored
        // Total is reproducible from the stored lines by anyone re-adding them — including
        // the cumulative claw-back formula, which compares a sum of returned line values
        // against this number and would otherwise drift by a fraction of a fils.
        var total = items.Sum(item => item.LineTotal);
        var pointsEarned = ValidateRedemptionAndEarn(total, pointsRedeemed, out var cashPaid);

        var order = new Order
        {
            CustomerId = customerId,
            EmployeeId = employeeId,
            Total = total,
            PointsEarned = pointsEarned,
            PointsRedeemed = pointsRedeemed,
            Status = OrderStatus.Completed,
            IdempotencyKey = key,
            CreatedAt = DateTimeOffset.UtcNow,
            OrderItems = items.Select(item => item.Entity).ToList()
        };

        _db.Orders.Add(order);

        try
        {
            // Saved before the points rows so the generated OrderId exists to link them to.
            // Still the same transaction — nothing is visible to another connection yet.
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (key is not null)
        {
            // Confirm, then translate (decision 28): the failure only means "already created"
            // once the winning row is actually there. Anything else — a check constraint, a
            // truncation, a dead connection — is re-thrown and becomes a 500 rather than being
            // reported to the cashier as a successful order that does not exist.
            var winner = await RecoverConcurrentCreateAsync(transaction, key, cancellationToken);

            if (winner is null)
            {
                throw;
            }

            _logger.LogWarning(
                ex,
                "Concurrent order creation for one Idempotency-Key; returning the winning order {OrderId}.",
                winner.OrderId);

            return winner;
        }

        await ApplyPointsAsync(order, cancellationToken);

        var newBalance = await _db.Customers
            .Where(c => c.Id == customerId)
            .Select(c => c.PointsBalance)
            .FirstAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new CreateOrderResponse
        {
            OrderId = order.Id,
            Total = total,
            CashPaid = cashPaid,
            PointsRedeemed = pointsRedeemed,
            PointsEarned = pointsEarned,
            NewBalance = newBalance
        };
    }

    /// <inheritdoc />
    public async Task<OrderDetailResponse> GetByIdAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        // Projected in SQL: the two names come out of the joins the navigations imply,
        // so neither the Customer nor the Employee row is materialized to read one field.
        var order = await _db.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new OrderDetailResponse
            {
                OrderId = o.Id,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                Total = o.Total,
                PointsRedeemed = o.PointsRedeemed,
                PointsEarned = o.PointsEarned,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.FullName,
                EmployeeName = o.Employee.FullName,
                Items = o.OrderItems
                    .OrderBy(item => item.Id)
                    .Select(item => new OrderItemResponse
                    {
                        OrderItemId = item.Id,
                        ProductName = item.ProductNameSnapshot,
                        Quantity = item.Quantity,
                        ReturnedQuantity = item.ReturnedQuantity,
                        UnitPriceSnapshot = item.UnitPriceSnapshot
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return order ?? throw ApiException.OrderNotFound();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderResponse>> GetForCustomerAsync(
        int customerId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        // An unknown customer and one who has simply never bought anything both produce no
        // rows, and they are not the same answer: the first is a bad id the cashier should
        // be told about, the second is an empty history. Hence the explicit existence check.
        if (!await _db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken))
        {
            throw ApiException.CustomerNotFound();
        }

        return await _db.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            // Newest first by id, which is insertion order and therefore chronological.
            // CreatedAt would tie for two orders rung up in the same instant; the key cannot.
            .OrderByDescending(o => o.Id)
            .Take(limit)
            .Select(o => new OrderResponse
            {
                OrderId = o.Id,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                Total = o.Total,
                PointsRedeemed = o.PointsRedeemed,
                PointsEarned = o.PointsEarned,
                Items = o.OrderItems
                    .OrderBy(item => item.Id)
                    .Select(item => new OrderItemResponse
                    {
                        OrderItemId = item.Id,
                        ProductName = item.ProductNameSnapshot,
                        Quantity = item.Quantity,
                        ReturnedQuantity = item.ReturnedQuantity,
                        UnitPriceSnapshot = item.UnitPriceSnapshot
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CancelOrderResponse> CancelAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        // Locked before anything is read off it (decision 39): a second cancel on the same
        // order waits here and then re-reads the status this one is about to write, so the
        // guard below sees Cancelled instead of the stale Completed both would otherwise read.
        var order = await LockedOrder(orderId).FirstOrDefaultAsync(cancellationToken)
            ?? throw ApiException.OrderNotFound();

        if (order.Status == OrderStatus.Cancelled)
        {
            throw ApiException.OrderAlreadyCancelled();
        }

        // Two independent readings of the same fact. Decision 20 makes the first return set
        // the status, so the status alone should be enough — but the whole point of this
        // guard is to stop the same points being reversed twice, and a returned line that
        // somehow never moved the status would defeat a status-only check silently.
        var hasReturns = order.Status == OrderStatus.Returned
            || await _db.OrderItems.AnyAsync(
                item => item.OrderId == orderId && item.ReturnedQuantity > 0,
                cancellationToken);

        if (hasReturns)
        {
            throw ApiException.OrderHasReturns();
        }

        EnsureWithinReturnWindow(order.CreatedAt);

        var newBalance = await ReverseOrderPointsAsync(
            order,
            clawBack: order.PointsEarned,
            restore: order.PointsRedeemed,
            cancellationToken);

        order.Status = OrderStatus.Cancelled;

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CancelOrderResponse
        {
            PointsClawedBack = order.PointsEarned,
            PointsRestored = order.PointsRedeemed,
            NewBalance = newBalance
        };
    }

    /// <inheritdoc />
    public async Task<ReturnOrderResponse> ReturnItemsAsync(
        int orderId,
        ReturnOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        // Locked before anything is read off it (decision 39). Two returns racing on the same
        // order would otherwise both read the same ReturnedQuantity values and both claw back
        // against them, taking the points for one line twice.
        var order = await LockedOrder(orderId)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw ApiException.OrderNotFound();

        if (order.Status == OrderStatus.Cancelled)
        {
            throw ApiException.OrderAlreadyCancelled();
        }

        // Decision 19: a redemption cannot be handed back in fractions, so an order that
        // spent points has exactly one reversal path — full cancellation.
        if (order.PointsRedeemed > 0)
        {
            throw ApiException.OrderPaidWithPoints();
        }

        EnsureWithinReturnWindow(order.CreatedAt);

        var refundAmount = ApplyReturnedQuantities(order, request.Items);

        // The cumulative claw-back (api-contract.md §5). Computed against the order's whole
        // returned value, not this request's, so no rounding error can accumulate across
        // returns: whatever was already taken is subtracted rather than re-derived.
        var returnedValueSoFar = order.OrderItems.Sum(ReturnedValueOf);

        var alreadyClawedBack = -await _db.PointsTransactions
            .Where(pt => pt.OrderId == orderId && pt.Type == PointsTransactionType.Refund)
            .SumAsync(pt => pt.Amount, cancellationToken);

        // Total is only ever 0 if every line rounded to nothing, in which case PointsEarned
        // is 0 too and there is nothing proportional to claw back. Guarded so the degenerate
        // case is an honest zero rather than a divide-by-zero 500.
        var targetClawBack = order.Total > 0
            ? (int)decimal.Floor(order.PointsEarned * returnedValueSoFar / order.Total)
            : 0;

        // Structurally non-negative: returnedValueSoFar only grows, PointsEarned and Total
        // never change (decision 22), so targetClawBack is monotonic and alreadyClawedBack
        // is simply its previous value. Clamped anyway — a negative here would silently
        // *grant* points, which is the one outcome a return must never produce.
        var pointsClawedBack = Math.Max(0, targetClawBack - alreadyClawedBack);

        if (pointsClawedBack > 0)
        {
            await DeductPointsAsync(order, pointsClawedBack, DateTimeOffset.UtcNow, cancellationToken);
        }

        // Decision 20: the first return moves Completed → Returned, and it is one-way. Whether
        // the order ends up partly or wholly returned is derived from the lines, not stored.
        if (order.Status == OrderStatus.Completed)
        {
            order.Status = OrderStatus.Returned;
        }

        await _db.SaveChangesAsync(cancellationToken);

        var newBalance = await _db.Customers
            .Where(c => c.Id == order.CustomerId)
            .Select(c => c.PointsBalance)
            .FirstAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new ReturnOrderResponse
        {
            RefundAmount = refundAmount,
            PointsClawedBack = pointsClawedBack,
            NewBalance = newBalance
        };
    }

    /// <summary>
    /// The order row, taken under an update lock held to the end of the calling transaction
    /// (decision 39). Every write path on an order starts here.
    /// </summary>
    /// <param name="orderId">The order's id.</param>
    /// <returns>A query for that one order, composable with <c>Include</c> as usual.</returns>
    /// <remarks>
    /// <c>UPDLOCK</c> makes the read itself exclusive against another writer's read, and
    /// <c>HOLDLOCK</c> keeps it until commit rather than releasing it when the statement ends.
    /// Without both, cancellation and returns are read-then-write under READ COMMITTED: two
    /// operations on the same order both see <c>Completed</c>, both pass the guards, and both
    /// reverse the same points. Decision 11's conditional balance UPDATE does not catch that —
    /// each claw-back is individually affordable, so the balance and the ledger end up wrong
    /// by the same amount and even the decision 21 verification query reconciles cleanly.
    /// <para>
    /// The second caller blocks rather than failing, then re-reads the state the first one
    /// committed, so <c>ORDER_ALREADY_CANCELLED</c> / <c>ORDER_HAS_RETURNS</c> reject it on
    /// the ordinary path. No new error code exists for the race, because the client cannot
    /// tell it from a plain double-tap and would do the same thing either way.
    /// </para>
    /// <para>
    /// Two writers can only ever queue on this one row and in this one order, so the lock
    /// cannot deadlock against itself. It is deliberately not taken on the read paths —
    /// <see cref="GetByIdAsync"/> and <see cref="GetForCustomerAsync"/> stay lock-free.
    /// </para>
    /// </remarks>
    private IQueryable<Order> LockedOrder(int orderId) =>
        _db.Orders.FromSql($"SELECT * FROM [Orders] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {orderId}");

    /// <summary>
    /// Rejects an <c>Idempotency-Key</c> the column cannot hold, and reduces an absent or
    /// blank header to "no key at all".
    /// </summary>
    /// <param name="key">The header value exactly as sent, or null when it was omitted.</param>
    /// <returns>The trimmed key, or null when the caller supplied none.</returns>
    /// <remarks>
    /// Length is checked here rather than left to SQL Server, which would answer an
    /// over-long key with a truncation error and a 500 instead of the contract's error body
    /// (the same reasoning as decision 31 for the price column). A whitespace-only header is
    /// treated as absent: it carries no identity, so honouring it would make every such
    /// request a replay of the first one.
    /// </remarks>
    /// <exception cref="ApiException">400 <c>VALIDATION_ERROR</c>.</exception>
    private static string? NormalizeIdempotencyKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var trimmed = key.Trim();

        if (trimmed.Length > Order.IdempotencyKeyMaxLength)
        {
            throw ApiException.ValidationError(
                $"مفتاح Idempotency-Key يقبل {Order.IdempotencyKeyMaxLength} حرفاً كحد أقصى");
        }

        return trimmed;
    }

    /// <summary>
    /// Rebuilds the receipt of an order that already carries this key, so a retry is answered
    /// with the first attempt's result instead of ringing the sale up again (decision 40).
    /// </summary>
    /// <param name="key">The normalized idempotency key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The original order's receipt, or null when no order carries the key.</returns>
    /// <remarks>
    /// Every figure but one is read straight off the immutable order row (decision 22), so the
    /// replay reports exactly what the first attempt reported. <c>NewBalance</c> is the
    /// exception: it is the customer's balance *now*, which is the honest answer — the balance
    /// is not an order figure and may have moved since. Displaying a stale one would be worse
    /// than displaying a different one.
    /// </remarks>
    private async Task<CreateOrderResponse?> FindByIdempotencyKeyAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var existing = await _db.Orders
            .AsNoTracking()
            .Where(o => o.IdempotencyKey == key)
            .Select(o => new
            {
                o.Id,
                o.Total,
                o.PointsRedeemed,
                o.PointsEarned,
                o.Customer.PointsBalance
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            return null;
        }

        return new CreateOrderResponse
        {
            OrderId = existing.Id,
            Total = existing.Total,
            CashPaid = existing.Total - (decimal)existing.PointsRedeemed / LoyaltyConstants.RedeemRate,
            PointsRedeemed = existing.PointsRedeemed,
            PointsEarned = existing.PointsEarned,
            NewBalance = existing.PointsBalance
        };
    }

    /// <summary>
    /// Winds this attempt back after the idempotency index rejected it, and reads the order
    /// the winning attempt committed.
    /// </summary>
    /// <param name="transaction">This attempt's transaction, to be abandoned.</param>
    /// <param name="key">The normalized idempotency key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The winner's receipt, or null when there is no winner and the caller should re-throw.</returns>
    /// <remarks>
    /// The rollback comes first for two reasons: this attempt's half-written order must not
    /// survive, and the winner's row was committed on another connection, so it is invisible
    /// from inside a transaction that started before it. Clearing the change tracker afterwards
    /// drops the rejected Order and its lines — the same detach decision 28 does, extended to
    /// the whole graph because an order is never added on its own.
    /// </remarks>
    private async Task<CreateOrderResponse?> RecoverConcurrentCreateAsync(
        IDbContextTransaction transaction,
        string key,
        CancellationToken cancellationToken)
    {
        await transaction.RollbackAsync(cancellationToken);
        _db.ChangeTracker.Clear();

        return await FindByIdempotencyKeyAsync(key, cancellationToken);
    }

    /// <summary>
    /// Validates the requested lines against the order and moves their
    /// <c>ReturnedQuantity</c> up.
    /// </summary>
    /// <param name="order">The order, with its lines loaded and tracked.</param>
    /// <param name="requested">The lines the client wants to return.</param>
    /// <returns>The cash value of this return, in JOD.</returns>
    /// <remarks>
    /// Two entries naming the same line are summed rather than rejected: they describe one
    /// line coming back in two pieces, and summing is what makes the remaining-quantity
    /// check see the real total instead of letting each half pass on its own.
    /// </remarks>
    /// <exception cref="ApiException">
    /// 400 <c>ITEM_NOT_IN_ORDER</c>, <c>RETURN_EXCEEDS_QUANTITY</c>, or <c>VALIDATION_ERROR</c>
    /// for a quantity finer than the column's three decimals.
    /// </exception>
    private static decimal ApplyReturnedQuantities(Order order, List<ReturnOrderItemRequest> requested)
    {
        var lines = order.OrderItems.ToDictionary(item => item.Id);

        var totals = requested
            .GroupBy(line => line.OrderItemId!.Value)
            .ToDictionary(group => group.Key, group => group.Sum(line => line.Quantity!.Value));

        var refundAmount = 0m;

        foreach (var (orderItemId, quantity) in totals)
        {
            if (!lines.TryGetValue(orderItemId, out var item))
            {
                throw ApiException.ItemNotInOrder();
            }

            // A fourth decimal would be rounded away by the column while the claw-back had
            // already been computed from the unrounded value — the stored ReturnedQuantity
            // and the points taken for it would then disagree for the rest of the order's life.
            if (decimal.Round(quantity, MoneyScale) != quantity)
            {
                throw ApiException.ValidationError("الكمية المرتجعة تقبل ثلاث خانات عشرية كحد أقصى");
            }

            if (quantity > item.Quantity - item.ReturnedQuantity)
            {
                throw ApiException.ReturnExceedsQuantity();
            }

            item.ReturnedQuantity += quantity;
            refundAmount += decimal.Round(quantity * item.UnitPriceSnapshot, MoneyScale, MidpointRounding.AwayFromZero);
        }

        return refundAmount;
    }

    /// <summary>
    /// The value already returned on one line, rounded the same way the line's value was
    /// rounded into <c>Order.Total</c> at creation.
    /// </summary>
    /// <param name="item">The order line.</param>
    /// <returns><c>ReturnedQuantity × UnitPriceSnapshot</c>, to the money scale.</returns>
    /// <remarks>
    /// Sharing the rounding with order creation is what makes the claw-back exact: when
    /// every line is fully returned, <c>ReturnedQuantity == Quantity</c>, so this sum is
    /// <c>Total</c> to the last fils, the ratio is exactly 1, and the floor lands on
    /// <c>PointsEarned</c> rather than one point under it.
    /// </remarks>
    private static decimal ReturnedValueOf(OrderItem item) =>
        decimal.Round(item.ReturnedQuantity * item.UnitPriceSnapshot, MoneyScale, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Writes a cancellation's two reversing movements and moves the balance to match.
    /// </summary>
    /// <param name="order">The order being cancelled.</param>
    /// <param name="clawBack">Points to take back — a magnitude, written as a negative <c>Refund</c>.</param>
    /// <param name="restore">Points to hand back — a magnitude, written as a positive <c>RedeemReversal</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The balance after both movements.</returns>
    /// <remarks>
    /// The restoration is applied <i>before</i> the claw-back, and the order matters. A
    /// cancellation is one operation whose net effect on an order that redeemed points is
    /// positive — the customer is being made whole. Clawing back first would reject, with
    /// <c>INSUFFICIENT_BALANCE_FOR_RETURN</c>, a cancellation that ends on a perfectly
    /// healthy balance: an order that redeemed 250 and earned 25 nets +225, yet a customer
    /// who has since spent down to 10 points could not afford the 25 in isolation. Restoring
    /// first checks the claw-back against the balance the customer actually has at that
    /// moment in the operation, which is what decision 8's "never a negative balance" means
    /// here. The rejection still stands whenever the finished operation would go negative.
    /// </remarks>
    /// <exception cref="ApiException">400 <c>INSUFFICIENT_BALANCE_FOR_RETURN</c>.</exception>
    private async Task<int> ReverseOrderPointsAsync(
        Order order,
        int clawBack,
        int restore,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        if (restore > 0)
        {
            await _db.Customers
                .Where(c => c.Id == order.CustomerId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.PointsBalance, c => c.PointsBalance + restore),
                    cancellationToken);

            _db.PointsTransactions.Add(new PointsTransaction
            {
                CustomerId = order.CustomerId,
                OrderId = order.Id,
                Amount = restore,
                Type = PointsTransactionType.RedeemReversal,
                CreatedAt = now
            });
        }

        if (clawBack > 0)
        {
            await DeductPointsAsync(order, clawBack, now, cancellationToken);
        }

        // Read back inside the transaction: ExecuteUpdate goes straight to SQL, so the
        // tracker holds no version of this row to be stale.
        return await _db.Customers
            .Where(c => c.Id == order.CustomerId)
            .Select(c => c.PointsBalance)
            .FirstAsync(cancellationToken);
    }

    /// <summary>
    /// Takes points back with the conditional atomic UPDATE decision 11 requires, and
    /// records the matching negative <c>Refund</c>.
    /// </summary>
    /// <param name="order">The order the movement belongs to.</param>
    /// <param name="amount">Points to deduct, as a positive magnitude.</param>
    /// <param name="occurredAt">Timestamp for the ledger row.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// The rows-affected count <i>is</i> the balance check: the comparison and the deduction
    /// are one statement, so no second till can slip between them.
    /// </remarks>
    /// <exception cref="ApiException">400 <c>INSUFFICIENT_BALANCE_FOR_RETURN</c> when the deduction matches no row.</exception>
    private async Task DeductPointsAsync(
        Order order,
        int amount,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var affected = await _db.Customers
            .Where(c => c.Id == order.CustomerId && c.PointsBalance >= amount)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(c => c.PointsBalance, c => c.PointsBalance - amount),
                cancellationToken);

        if (affected == 0)
        {
            throw ApiException.InsufficientBalanceForReturn();
        }

        _db.PointsTransactions.Add(new PointsTransaction
        {
            CustomerId = order.CustomerId,
            OrderId = order.Id,
            Amount = -amount,
            Type = PointsTransactionType.Refund,
            CreatedAt = occurredAt
        });
    }

    /// <summary>
    /// Enforces the return window: the end of the calendar day following the order, in
    /// Jordan time (decision 16).
    /// </summary>
    /// <param name="orderCreatedAt">The order's creation instant, with its stored offset.</param>
    /// <remarks>
    /// The deadline is built as the <i>start</i> of the day after the last allowed day and
    /// compared with a strict <c>&lt;</c>. That is the same boundary as "23:59:59.999" without
    /// having to name a last representable instant — a comparison against 23:59:59.999 would
    /// silently accept the sub-millisecond sliver above it.
    /// <para>
    /// Note this is not a rolling 24 hours: an order at 23:00 has about 25 hours, one at
    /// 08:00 about 40. The variable length is deliberate and always favours the customer.
    /// </para>
    /// </remarks>
    /// <exception cref="ApiException">400 <c>RETURN_WINDOW_EXPIRED</c>.</exception>
    private static void EnsureWithinReturnWindow(DateTimeOffset orderCreatedAt)
    {
        var offset = LoyaltyConstants.JordanUtcOffset;

        var orderDayInJordan = orderCreatedAt.ToOffset(offset).Date;
        var firstDayAfterWindow = orderDayInJordan.AddDays(LoyaltyConstants.ReturnWindowDays + 1);
        var deadline = new DateTimeOffset(firstDayAfterWindow, offset);

        if (DateTimeOffset.UtcNow >= deadline)
        {
            throw ApiException.ReturnWindowExpired();
        }
    }

    /// <summary>
    /// Turns the requested lines into OrderItems carrying the catalog's own name and price.
    /// </summary>
    /// <param name="requested">The lines as the client sent them.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>One entry per requested line, in the order they were sent.</returns>
    /// <remarks>
    /// Every line is resolved and validated before the caller writes anything, so a basket
    /// with a bad product in the middle fails having touched nothing. The transaction would
    /// undo a partial write in any case; this only means there is nothing to undo.
    /// <para>
    /// Repeated product ids are kept as separate lines rather than merged: the cashier
    /// scanning the same item twice is describing two lines, and merging them would make
    /// the receipt disagree with what was rung up. Each line returns independently later.
    /// </para>
    /// </remarks>
    /// <exception cref="ApiException">404 <c>PRODUCT_NOT_FOUND</c>; 400 <c>PRODUCT_UNAVAILABLE</c> or <c>INVALID_QUANTITY</c>.</exception>
    private async Task<List<PricedItem>> BuildItemsAsync(
        List<CreateOrderItemRequest> requested,
        CancellationToken cancellationToken)
    {
        var productIds = requested.Select(item => item.ProductId!.Value).Distinct().ToList();

        // One query for the whole basket; a soft-deleted product is simply absent, which
        // lands on PRODUCT_NOT_FOUND exactly as the catalog module treats it (decision 14).
        var products = await _db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var items = new List<PricedItem>(requested.Count);

        foreach (var line in requested)
        {
            if (!products.TryGetValue(line.ProductId!.Value, out var product))
            {
                throw ApiException.ProductNotFound();
            }

            if (!product.IsAvailable)
            {
                throw ApiException.ProductUnavailable();
            }

            var quantity = ValidateQuantity(line.Quantity!.Value, product);

            items.Add(new PricedItem(
                new OrderItem
                {
                    ProductId = product.Id,
                    ProductNameSnapshot = product.Name,
                    Quantity = quantity,
                    ReturnedQuantity = 0,
                    UnitPriceSnapshot = product.Price
                },
                decimal.Round(quantity * product.Price, MoneyScale, MidpointRounding.AwayFromZero)));
        }

        return items;
    }

    /// <summary>
    /// Rejects a quantity the column cannot hold exactly, or one the product cannot be
    /// sold in. The lower and upper bounds are already enforced by the DTO's <c>Range</c>.
    /// </summary>
    /// <param name="quantity">The submitted quantity.</param>
    /// <param name="product">The catalog row, which decides whether fractions are meaningful.</param>
    /// <returns>The same quantity, once it is known to be sellable and storable.</returns>
    /// <exception cref="ApiException">400 <c>INVALID_QUANTITY</c>.</exception>
    private static decimal ValidateQuantity(decimal quantity, Product product)
    {
        // Same reasoning as the catalog's price scale: SQL Server would round a fourth
        // decimal away silently, and the receipt would then not add up to what was charged.
        if (decimal.Round(quantity, MoneyScale) != quantity)
        {
            throw ApiException.InvalidQuantity("الكمية تقبل ثلاث خانات عشرية كحد أقصى");
        }

        // A weight can be 1.5 kg; a count cannot be 1.5 cups (contract's INVALID_QUANTITY).
        if (product.UnitType == UnitType.Piece && decimal.Truncate(quantity) != quantity)
        {
            throw ApiException.InvalidQuantity("الكمية يجب أن تكون عدداً صحيحاً لهذا المنتج");
        }

        return quantity;
    }

    /// <summary>
    /// Applies the redemption rules and computes what the order earns (ERD core formulas).
    /// </summary>
    /// <param name="total">The order's full value.</param>
    /// <param name="pointsRedeemed">Points the customer wants to spend.</param>
    /// <param name="cashPaid">The amount actually collected at the till.</param>
    /// <returns>Points earned — floored, and on <paramref name="cashPaid"/> only (decision 9).</returns>
    /// <remarks>
    /// <c>INSUFFICIENT_BALANCE</c> is deliberately not checked here. Reading the balance and
    /// then deducting it are two steps a second till can interleave with; the deduction
    /// itself is the check (decision 11), and it happens in <see cref="ApplyPointsAsync"/>.
    /// </remarks>
    /// <exception cref="ApiException">400 <c>REDEEM_BELOW_MINIMUM</c> or <c>REDEEM_EXCEEDS_TOTAL</c>.</exception>
    private static int ValidateRedemptionAndEarn(decimal total, int pointsRedeemed, out decimal cashPaid)
    {
        if (pointsRedeemed > 0 && pointsRedeemed < LoyaltyConstants.MinRedeemPoints)
        {
            throw ApiException.RedeemBelowMinimum();
        }

        var redemptionValue = (decimal)pointsRedeemed / LoyaltyConstants.RedeemRate;

        if (redemptionValue > total)
        {
            throw ApiException.RedeemExceedsTotal();
        }

        cashPaid = total - redemptionValue;

        // Points are earned on cash only, so redeemed points never mint more points
        // (decision 9). Floor, never round: the shop does not give away a fraction.
        return (int)decimal.Floor(cashPaid * LoyaltyConstants.PointsPerDinar);
    }

    /// <summary>
    /// Writes the order's points movements and moves the balance to match them.
    /// </summary>
    /// <param name="order">The saved order, already carrying its id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Redemption is applied before earning, so the points this order grants can never fund
    /// the redemption on the same order. Both use the statement forms decision 11 requires:
    /// a conditional decrement whose rows-affected count <i>is</i> the balance check, and a
    /// plain increment. Neither reads the balance first, so two tills ringing up the same
    /// customer at once cannot both pass a check that only one of them can honour.
    /// <para>
    /// A movement worth zero points writes no row. It would carry no information, and the
    /// invariant <c>PointsBalance == Σ Amount</c> is indifferent to a zero.
    /// </para>
    /// </remarks>
    /// <exception cref="ApiException">400 <c>INSUFFICIENT_BALANCE</c> when the deduction matches no row.</exception>
    private async Task ApplyPointsAsync(Order order, CancellationToken cancellationToken)
    {
        var customerId = order.CustomerId;
        var now = DateTimeOffset.UtcNow;

        if (order.PointsRedeemed > 0)
        {
            var redeemed = order.PointsRedeemed;

            var affected = await _db.Customers
                .Where(c => c.Id == customerId && c.PointsBalance >= redeemed)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.PointsBalance, c => c.PointsBalance - redeemed),
                    cancellationToken);

            if (affected == 0)
            {
                throw ApiException.InsufficientBalance();
            }

            _db.PointsTransactions.Add(new PointsTransaction
            {
                CustomerId = customerId,
                OrderId = order.Id,
                Amount = -redeemed,
                Type = PointsTransactionType.Redeem,
                CreatedAt = now
            });
        }

        if (order.PointsEarned > 0)
        {
            var earned = order.PointsEarned;

            await _db.Customers
                .Where(c => c.Id == customerId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.PointsBalance, c => c.PointsBalance + earned),
                    cancellationToken);

            _db.PointsTransactions.Add(new PointsTransaction
            {
                CustomerId = customerId,
                OrderId = order.Id,
                Amount = earned,
                Type = PointsTransactionType.Earn,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// A resolved basket line: the OrderItem about to be written, and the line's value in
    /// JOD rounded to fils.
    /// </summary>
    /// <param name="Entity">The OrderItem, already carrying its price and name snapshots.</param>
    /// <param name="LineTotal">Quantity × unit price, rounded to the money scale.</param>
    private sealed record PricedItem(OrderItem Entity, decimal LineTotal);
}
