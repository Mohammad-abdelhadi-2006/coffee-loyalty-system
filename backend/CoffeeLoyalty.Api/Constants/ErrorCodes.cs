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

    /// <summary>404 — no active product with that id (a soft-deleted product is gone as far as the API is concerned).</summary>
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";

    /// <summary>404 — no customer with that id or normalized phone number.</summary>
    public const string CustomerNotFound = "CUSTOMER_NOT_FOUND";

    /// <summary>409 — registering a phone number that already identifies a customer.</summary>
    public const string PhoneAlreadyExists = "PHONE_ALREADY_EXISTS";

    /// <summary>404 — no employee with that id.</summary>
    public const string EmployeeNotFound = "EMPLOYEE_NOT_FOUND";

    /// <summary>401 — no token, or a token the JWT middleware rejected.</summary>
    public const string Unauthorized = "UNAUTHORIZED";

    /// <summary>403 — valid token, but the role is not allowed on this endpoint.</summary>
    public const string Forbidden = "FORBIDDEN";

    /// <summary>429 — too many failed authentication attempts against one account (decision 27).</summary>
    public const string TooManyRequests = "TOO_MANY_REQUESTS";

    /// <summary>500 — unhandled server-side failure. Details are logged, never returned.</summary>
    public const string InternalError = "INTERNAL_ERROR";
}
