using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CoffeeLoyalty.Api.Extensions;

/// <summary>
/// Reads the caller's identity out of the validated token, so that no endpoint has to
/// spell the claim name out and none of them can disagree about which claim carries it.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The signed-in user's row id, from the <c>sub</c> claim.
    /// </summary>
    /// <param name="principal">The current principal.</param>
    /// <returns>The customer's or employee's <c>Id</c>, depending on the token's role.</returns>
    /// <remarks>
    /// Never a client error: <c>AddCoffeeLoyaltyAuth</c>'s <c>OnTokenValidated</c> already
    /// fails any token whose <c>sub</c> is missing or unparseable, so a principal that
    /// reaches an action always has one. A failure here means that check was bypassed —
    /// a bug, which the exception middleware turns into a 500 rather than into a 401 that
    /// would invite the caller to retry something that can never succeed.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The principal carries no usable <c>sub</c> claim.</exception>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(subject, out var userId))
        {
            throw new InvalidOperationException(
                "The authenticated principal carries no parseable 'sub' claim; token validation should have rejected it.");
        }

        return userId;
    }
}
