using System.Security.Claims;
using System.Text;
using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Constants;
using CoffeeLoyalty.Api.Data;
using CoffeeLoyalty.Api.Options;
using CoffeeLoyalty.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CoffeeLoyalty.Api.Extensions;

/// <summary>
/// Registration helpers that keep Program.cs readable.
/// </summary>
public static class AuthSetupExtensions
{
    /// <summary>
    /// Configures JWT bearer authentication and the three role policies from the
    /// contract's auth levels, and registers the authentication services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (User Secrets / environment).</param>
    /// <returns>The same collection, for chaining.</returns>
    /// <exception cref="InvalidOperationException">The <c>Jwt</c> or <c>LoginThrottle</c> section is missing or invalid.</exception>
    public static IServiceCollection AddCoffeeLoyaltyAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        // Fail at startup, not on the first login attempt.
        jwtOptions.Validate();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Same rule for the throttle: a misconfigured section must not be discovered
        // by a user who cannot log in (decision 27).
        var throttleOptions = configuration.GetSection(LoginThrottleOptions.SectionName).Get<LoginThrottleOptions>()
            ?? new LoginThrottleOptions();

        throttleOptions.Validate();

        services.Configure<LoginThrottleOptions>(configuration.GetSection(LoginThrottleOptions.SectionName));

        // Backing store for the login throttle.
        services.AddMemoryCache();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep the short `role` / `name` claim types the contract specifies
                // instead of letting the handler rewrite them into WS-* URIs.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),

                    // No grace period: an expired token is expired.
                    ClockSkew = TimeSpan.Zero,

                    RoleClaimType = JwtTokenService.RoleClaimType,
                    NameClaimType = JwtTokenService.NameClaimType
                };

                // The handler's default 401/403 bodies are empty, which would break the
                // contract's promise that *every* non-2xx carries { code, message }.
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        await new ApiError(ErrorCodes.Unauthorized, ErrorMessages.Unauthorized)
                            .WriteToAsync(context.Response, StatusCodes.Status401Unauthorized, context.HttpContext.RequestAborted);
                    },
                    OnForbidden = async context =>
                        await new ApiError(ErrorCodes.Forbidden, ErrorMessages.Forbidden)
                            .WriteToAsync(context.Response, StatusCodes.Status403Forbidden, context.HttpContext.RequestAborted),

                    // Signature and lifetime are not enough: the token is also checked
                    // against the current database state (decision 25).
                    OnTokenValidated = ValidateAgainstDatabaseAsync
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(RoleNames.Customer, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(RoleNames.Customer));

            // An admin can do everything a cashier can (api-contract.md → Auth levels).
            options.AddPolicy(RoleNames.Cashier, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(RoleNames.Cashier, RoleNames.Admin));

            options.AddPolicy(RoleNames.Admin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(RoleNames.Admin));
        });

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
        services.AddSingleton<IPasswordVerifier, BCryptPasswordVerifier>();
        services.AddSingleton<ILoginThrottle, LoginThrottle>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    /// <summary>
    /// Re-checks an otherwise valid token against the database (decision 25): the
    /// <c>TokenVersion</c> it was issued with must still match the stored value, and an
    /// employee must still be active. Costs one primary-key read per authenticated
    /// request — the price of being able to revoke a token at all.
    /// </summary>
    /// <param name="context">The token-validation context; <see cref="TokenValidatedContext.Fail(string)"/> turns into the usual 401 body via <c>OnChallenge</c>.</param>
    private static async Task ValidateAgainstDatabaseAsync(TokenValidatedContext context)
    {
        if (context.Principal is not { } principal
            || !int.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId)
            || !int.TryParse(principal.FindFirstValue(JwtTokenService.TokenVersionClaimType), out var tokenVersion))
        {
            // Correctly signed but missing the claims we validate on — a token this API
            // no longer issues, or one issued before decision 25.
            context.Fail("The token does not carry the claims required to validate it.");
            return;
        }

        // Scoped: the DbContext must come from this request, not from the singleton
        // container the JwtBearer options were configured in.
        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var cancellationToken = context.HttpContext.RequestAborted;

        if (principal.FindFirstValue(JwtTokenService.RoleClaimType) == RoleNames.Customer)
        {
            var storedVersion = await db.Customers
                .AsNoTracking()
                .Where(c => c.Id == userId)
                .Select(c => (int?)c.TokenVersion)
                .FirstOrDefaultAsync(cancellationToken);

            if (storedVersion != tokenVersion)
            {
                context.Fail("The customer's tokens were revoked, or the customer no longer exists.");
            }

            return;
        }

        var employee = await db.Employees
            .AsNoTracking()
            .Where(e => e.Id == userId)
            .Select(e => new { e.TokenVersion, e.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        // Re-reading IsActive here is what makes deactivation take effect on the next
        // request instead of after the 12 h token lifetime (decision 18).
        if (employee is null || employee.TokenVersion != tokenVersion || !employee.IsActive)
        {
            context.Fail("The employee's tokens were revoked, or the account is deactivated or gone.");
        }
    }

    /// <summary>
    /// Replaces MVC's default <c>ValidationProblemDetails</c> body with the unified
    /// error shape, so a malformed request looks like every other failure.
    /// </summary>
    /// <param name="builder">The MVC builder.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IMvcBuilder UseUnifiedValidationErrors(this IMvcBuilder builder) =>
        builder.ConfigureApiBehaviorOptions(options =>
        {
            // The English model-state text is for the developer, not the user — it is
            // deliberately dropped rather than translated into the response body.
            options.InvalidModelStateResponseFactory = _ =>
                new ObjectResult(new ApiError(ErrorCodes.ValidationError, ErrorMessages.ValidationError))
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
        });
}
