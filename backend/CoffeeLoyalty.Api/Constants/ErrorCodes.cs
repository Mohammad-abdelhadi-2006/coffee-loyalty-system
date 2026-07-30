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

    /// <summary>401 — no token, or a token the JWT middleware rejected.</summary>
    public const string Unauthorized = "UNAUTHORIZED";

    /// <summary>403 — valid token, but the role is not allowed on this endpoint.</summary>
    public const string Forbidden = "FORBIDDEN";

    /// <summary>500 — unhandled server-side failure. Details are logged, never returned.</summary>
    public const string InternalError = "INTERNAL_ERROR";
}
