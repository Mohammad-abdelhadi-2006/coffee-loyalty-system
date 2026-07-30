using System.Text.RegularExpressions;

namespace CoffeeLoyalty.Api.Common;

/// <summary>
/// Normalizes Jordanian mobile numbers to E.164. The phone number is the customer's
/// identity (ERD) and the key the Firebase account is linked by (decision 11), so
/// every entry point must reduce it to exactly one canonical form — otherwise
/// "0791234567" and "+962791234567" become two different customers.
/// </summary>
public static partial class JordanPhoneNumber
{
    private const string CountryCode = "962";

    /// <summary>The national significant number: a mobile <c>7</c> followed by 8 digits.</summary>
    [GeneratedRegex(@"^7\d{8}$")]
    private static partial Regex NationalNumber();

    /// <summary>Anything that is not a digit or a leading plus — spaces, dashes, parentheses.</summary>
    [GeneratedRegex(@"[^\d+]")]
    private static partial Regex Noise();

    /// <summary>
    /// Converts any accepted Jordanian mobile spelling to E.164.
    /// Accepts <c>07XXXXXXXX</c>, <c>7XXXXXXXX</c>, <c>+9627XXXXXXXX</c> and
    /// <c>009627XXXXXXXX</c>, with or without separators.
    /// </summary>
    /// <param name="input">The raw number as typed or as received from Firebase.</param>
    /// <param name="normalized">The canonical <c>+9627XXXXXXXX</c> form, or null when invalid.</param>
    /// <returns><c>true</c> when the input is a valid Jordanian mobile number.</returns>
    public static bool TryNormalize(string? input, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var digits = Noise().Replace(input.Trim(), string.Empty).TrimStart('+');

        // Strip the country code in any of its spellings, leaving the national number.
        if (digits.StartsWith("00" + CountryCode, StringComparison.Ordinal))
        {
            digits = digits[(2 + CountryCode.Length)..];
        }
        else if (digits.StartsWith(CountryCode, StringComparison.Ordinal))
        {
            digits = digits[CountryCode.Length..];
        }

        // Domestic trunk prefix: 07XXXXXXXX -> 7XXXXXXXX.
        if (digits.StartsWith('0'))
        {
            digits = digits[1..];
        }

        if (!NationalNumber().IsMatch(digits))
        {
            return false;
        }

        normalized = $"+{CountryCode}{digits}";
        return true;
    }

    /// <summary>
    /// Normalizes or throws the contract's <c>INVALID_PHONE</c> error.
    /// </summary>
    /// <param name="input">The raw number.</param>
    /// <returns>The canonical <c>+9627XXXXXXXX</c> form.</returns>
    /// <exception cref="ApiException">400 <c>INVALID_PHONE</c> when the number is not a Jordanian mobile.</exception>
    public static string NormalizeOrThrow(string? input)
    {
        if (!TryNormalize(input, out var normalized) || normalized is null)
        {
            throw ApiException.InvalidPhone();
        }

        return normalized;
    }
}
