using System.Text;
using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Constants;
using CoffeeLoyalty.Api.Options;
using CoffeeLoyalty.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
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
    /// <exception cref="InvalidOperationException">The <c>Jwt</c> section is missing or weak.</exception>
    public static IServiceCollection AddCoffeeLoyaltyAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        // Fail at startup, not on the first login attempt.
        jwtOptions.Validate();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

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
                            .WriteToAsync(context.Response, StatusCodes.Status403Forbidden, context.HttpContext.RequestAborted)
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
        services.AddScoped<IAuthService, AuthService>();

        return services;
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
