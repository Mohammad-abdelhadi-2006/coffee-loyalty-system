using CoffeeLoyalty.Api.Constants;

namespace CoffeeLoyalty.Api.Common;

/// <summary>
/// A deliberate, client-facing failure. Carries everything the unified error
/// response needs: the registry code, the HTTP status, and the Arabic message.
/// Anything thrown that is <i>not</i> an <see cref="ApiException"/> is a bug and
/// becomes a generic 500 with no details leaked.
/// </summary>
public class ApiException : Exception
{
    /// <summary>
    /// Creates a client-facing failure.
    /// </summary>
    /// <param name="code">A stable constant from <see cref="ErrorCodes"/>.</param>
    /// <param name="statusCode">The HTTP status the registry assigns to that code.</param>
    /// <param name="arabicMessage">Display text for the client, from <see cref="ErrorMessages"/>.</param>
    public ApiException(string code, int statusCode, string arabicMessage)
        : base($"{code}: {arabicMessage}")
    {
        Code = code;
        StatusCode = statusCode;
        ArabicMessage = arabicMessage;
    }

    /// <summary>The machine-readable code clients branch on. Never changes.</summary>
    public string Code { get; }

    /// <summary>The HTTP status code to return.</summary>
    public int StatusCode { get; }

    /// <summary>The Arabic display text. May change freely.</summary>
    public string ArabicMessage { get; }

    /// <summary>401 — wrong username or password.</summary>
    public static ApiException InvalidCredentials() =>
        new(ErrorCodes.InvalidCredentials, StatusCodes.Status401Unauthorized, ErrorMessages.InvalidCredentials);

    /// <summary>401 — the employee account was deactivated (decision 18).</summary>
    public static ApiException AccountDisabled() =>
        new(ErrorCodes.AccountDisabled, StatusCodes.Status401Unauthorized, ErrorMessages.AccountDisabled);

    /// <summary>401 — the Firebase ID token could not be verified.</summary>
    public static ApiException InvalidFirebaseToken() =>
        new(ErrorCodes.InvalidFirebaseToken, StatusCodes.Status401Unauthorized, ErrorMessages.InvalidFirebaseToken);

    /// <summary>400 — the phone number is not a valid Jordanian mobile.</summary>
    public static ApiException InvalidPhone() =>
        new(ErrorCodes.InvalidPhone, StatusCodes.Status400BadRequest, ErrorMessages.InvalidPhone);

    /// <summary>429 — too many failed attempts against this account (decision 27).</summary>
    public static ApiException TooManyRequests() =>
        new(ErrorCodes.TooManyRequests, StatusCodes.Status429TooManyRequests, ErrorMessages.TooManyRequests);

    /// <summary>404 — no active product carries that id (decision 14: soft-deleted products stay hidden).</summary>
    public static ApiException ProductNotFound() =>
        new(ErrorCodes.ProductNotFound, StatusCodes.Status404NotFound, ErrorMessages.ProductNotFound);

    /// <summary>404 — no customer carries that id or phone number.</summary>
    public static ApiException CustomerNotFound() =>
        new(ErrorCodes.CustomerNotFound, StatusCodes.Status404NotFound, ErrorMessages.CustomerNotFound);

    /// <summary>409 — that phone number already identifies a customer (the phone is the identity, decision 11).</summary>
    public static ApiException PhoneAlreadyExists() =>
        new(ErrorCodes.PhoneAlreadyExists, StatusCodes.Status409Conflict, ErrorMessages.PhoneAlreadyExists);

    /// <summary>404 — no employee carries that id.</summary>
    public static ApiException EmployeeNotFound() =>
        new(ErrorCodes.EmployeeNotFound, StatusCodes.Status404NotFound, ErrorMessages.EmployeeNotFound);

    /// <summary>404 — no order carries that id.</summary>
    public static ApiException OrderNotFound() =>
        new(ErrorCodes.OrderNotFound, StatusCodes.Status404NotFound, ErrorMessages.OrderNotFound);

    /// <summary>400 — the order is already cancelled; there is nothing left to reverse.</summary>
    public static ApiException OrderAlreadyCancelled() =>
        new(ErrorCodes.OrderAlreadyCancelled, StatusCodes.Status400BadRequest, ErrorMessages.OrderAlreadyCancelled);

    /// <summary>400 — a return already happened, so cancelling would reverse the same points twice (decision 20).</summary>
    public static ApiException OrderHasReturns() =>
        new(ErrorCodes.OrderHasReturns, StatusCodes.Status400BadRequest, ErrorMessages.OrderHasReturns);

    /// <summary>400 — past the end of the day following the order, in Jordan time (decision 16).</summary>
    public static ApiException ReturnWindowExpired() =>
        new(ErrorCodes.ReturnWindowExpired, StatusCodes.Status400BadRequest, ErrorMessages.ReturnWindowExpired);

    /// <summary>400 — the balance does not cover the points to claw back (never a negative balance, decision 8).</summary>
    public static ApiException InsufficientBalanceForReturn() =>
        new(ErrorCodes.InsufficientBalanceForReturn, StatusCodes.Status400BadRequest, ErrorMessages.InsufficientBalanceForReturn);

    /// <summary>400 — an order paid with points can only be cancelled in full, never partially returned (decision 19).</summary>
    public static ApiException OrderPaidWithPoints() =>
        new(ErrorCodes.OrderPaidWithPoints, StatusCodes.Status400BadRequest, ErrorMessages.OrderPaidWithPoints);

    /// <summary>400 — that OrderItem belongs to a different order.</summary>
    public static ApiException ItemNotInOrder() =>
        new(ErrorCodes.ItemNotInOrder, StatusCodes.Status400BadRequest, ErrorMessages.ItemNotInOrder);

    /// <summary>400 — more was returned than the line has left.</summary>
    public static ApiException ReturnExceedsQuantity() =>
        new(ErrorCodes.ReturnExceedsQuantity, StatusCodes.Status400BadRequest, ErrorMessages.ReturnExceedsQuantity);

    /// <summary>400 — the product is on the menu but switched off at the counter (decision 14).</summary>
    public static ApiException ProductUnavailable() =>
        new(ErrorCodes.ProductUnavailable, StatusCodes.Status400BadRequest, ErrorMessages.ProductUnavailable);

    /// <summary>400 — the line's quantity is not a quantity this product can be sold in.</summary>
    /// <param name="arabicMessage">Optional detail; defaults to the generic quantity message.</param>
    public static ApiException InvalidQuantity(string? arabicMessage = null) =>
        new(ErrorCodes.InvalidQuantity, StatusCodes.Status400BadRequest, arabicMessage ?? ErrorMessages.InvalidQuantity);

    /// <summary>400 — a redemption below the shop owner's per-redemption minimum (decision 9).</summary>
    public static ApiException RedeemBelowMinimum() =>
        new(ErrorCodes.RedeemBelowMinimum, StatusCodes.Status400BadRequest, ErrorMessages.RedeemBelowMinimum);

    /// <summary>400 — the balance does not cover the points being spent (never a negative balance, decision 8).</summary>
    public static ApiException InsufficientBalance() =>
        new(ErrorCodes.InsufficientBalance, StatusCodes.Status400BadRequest, ErrorMessages.InsufficientBalance);

    /// <summary>400 — the redemption is worth more than the order, which would make cash paid negative (ERD constraint).</summary>
    public static ApiException RedeemExceedsTotal() =>
        new(ErrorCodes.RedeemExceedsTotal, StatusCodes.Status400BadRequest, ErrorMessages.RedeemExceedsTotal);

    /// <summary>400 — the request body failed validation.</summary>
    public static ApiException ValidationError(string? arabicMessage = null) =>
        new(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest, arabicMessage ?? ErrorMessages.ValidationError);

    /// <summary>400 — first login for an unknown phone number, with no name to create the customer from (decision 5).</summary>
    /// <param name="arabicMessage">Optional detail; defaults to the generic name-required message.</param>
    public static ApiException NameRequired(string? arabicMessage = null) =>
        new(ErrorCodes.NameRequired, StatusCodes.Status400BadRequest, arabicMessage ?? ErrorMessages.NameRequired);
}
