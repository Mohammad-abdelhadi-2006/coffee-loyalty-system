namespace CoffeeLoyalty.Api.Constants;

/// <summary>
/// The stable, machine-readable error codes from the registry in api-contract.md.
/// Clients build logic on these strings, so they never change. Only the codes the
/// authentication layer can actually emit are declared here; the rest are added
/// by the slices that raise them.
/// </summary>
public static class ErrorCodes
{
    /// <summary>401 — wrong username or password.</summary>
    public const string InvalidCredentials = "INVALID_CREDENTIALS";

    /// <summary>401 — the employee row has <c>IsActive = false</c> (decision 18).</summary>
    public const string AccountDisabled = "ACCOUNT_DISABLED";

    /// <summary>401 — the Firebase ID token failed verification.</summary>
    public const string InvalidFirebaseToken = "INVALID_FIREBASE_TOKEN";

    /// <summary>400 — not a valid Jordanian mobile number after E.164 normalization.</summary>
    public const string InvalidPhone = "INVALID_PHONE";

    /// <summary>400 — malformed request body (model validation).</summary>
    public const string ValidationError = "VALIDATION_ERROR";

    /// <summary>
    /// 400 — a customer token exchange for a phone number that has no customer yet,
    /// with no name to create one from. Distinct from <see cref="ValidationError"/> so
    /// the app can tell a first login apart from a malformed body and collect the name
    /// instead of showing an error (decision 5).
    /// </summary>
    public const string NameRequired = "NAME_REQUIRED";

    /// <summary>404 — no active product with that id (a soft-deleted product is gone as far as the API is concerned).</summary>
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";

    /// <summary>404 — no customer with that id or normalized phone number.</summary>
    public const string CustomerNotFound = "CUSTOMER_NOT_FOUND";

    /// <summary>409 — registering a phone number that already identifies a customer.</summary>
    public const string PhoneAlreadyExists = "PHONE_ALREADY_EXISTS";

    /// <summary>404 — no employee with that id.</summary>
    public const string EmployeeNotFound = "EMPLOYEE_NOT_FOUND";

    /// <summary>404 — no order with that id.</summary>
    public const string OrderNotFound = "ORDER_NOT_FOUND";

    /// <summary>400 — cancelling or returning against an already-cancelled order.</summary>
    public const string OrderAlreadyCancelled = "ORDER_ALREADY_CANCELLED";

    /// <summary>400 — full cancellation attempted after a partial return happened (decision 20).</summary>
    public const string OrderHasReturns = "ORDER_HAS_RETURNS";

    /// <summary>400 — past the end of the day following the order, Jordan time (decision 16).</summary>
    public const string ReturnWindowExpired = "RETURN_WINDOW_EXPIRED";

    /// <summary>400 — the claw-back would push the customer's balance below zero (decision 8).</summary>
    public const string InsufficientBalanceForReturn = "INSUFFICIENT_BALANCE_FOR_RETURN";

    /// <summary>400 — partial return on an order that was paid with points (decision 19).</summary>
    public const string OrderPaidWithPoints = "ORDER_PAID_WITH_POINTS";

    /// <summary>400 — the <c>orderItemId</c> does not belong to the order in the route.</summary>
    public const string ItemNotInOrder = "ITEM_NOT_IN_ORDER";

    /// <summary>400 — returning more than the line has left (<c>Quantity − ReturnedQuantity</c>).</summary>
    public const string ReturnExceedsQuantity = "RETURN_EXCEEDS_QUANTITY";

    /// <summary>400 — the product exists but the counter has it switched off (<c>IsAvailable = false</c>).</summary>
    public const string ProductUnavailable = "PRODUCT_UNAVAILABLE";

    /// <summary>400 — quantity ≤ 0, finer than the column's three decimals, or fractional for a <c>Piece</c> product.</summary>
    public const string InvalidQuantity = "INVALID_QUANTITY";

    /// <summary>400 — <c>0 &lt; pointsRedeemed &lt; MinRedeemPoints</c> (decision 9).</summary>
    public const string RedeemBelowMinimum = "REDEEM_BELOW_MINIMUM";

    /// <summary>400 — the customer's balance does not cover the points being spent.</summary>
    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";

    /// <summary>400 — the redeemed points are worth more than the order (cash paid would go negative).</summary>
    public const string RedeemExceedsTotal = "REDEEM_EXCEEDS_TOTAL";

    /// <summary>401 — no token, or a token the JWT middleware rejected.</summary>
    public const string Unauthorized = "UNAUTHORIZED";

    /// <summary>403 — valid token, but the role is not allowed on this endpoint.</summary>
    public const string Forbidden = "FORBIDDEN";

    /// <summary>429 — too many failed authentication attempts against one account (decision 27).</summary>
    public const string TooManyRequests = "TOO_MANY_REQUESTS";

    /// <summary>500 — unhandled server-side failure. Details are logged, never returned.</summary>
    public const string InternalError = "INTERNAL_ERROR";
}
