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

    /// <summary>400 — the request body failed validation.</summary>
    public static ApiException ValidationError(string? arabicMessage = null) =>
        new(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest, arabicMessage ?? ErrorMessages.ValidationError);
}
