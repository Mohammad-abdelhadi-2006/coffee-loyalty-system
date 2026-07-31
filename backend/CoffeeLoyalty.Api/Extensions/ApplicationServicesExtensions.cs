using CoffeeLoyalty.Api.Services;

namespace CoffeeLoyalty.Api.Extensions;

/// <summary>
/// Registration helper for the business services, keeping Program.cs to one line per
/// concern. The authentication services are registered separately in
/// <see cref="AuthSetupExtensions.AddCoffeeLoyaltyAuth"/>, because they come with their own
/// options validation and middleware wiring.
/// </summary>
public static class ApplicationServicesExtensions
{
    /// <summary>
    /// Registers the per-request services behind the CRUD modules.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, for chaining.</returns>
    /// <remarks>
    /// Scoped, not singleton: each one holds the request's <see cref="Data.AppDbContext"/>,
    /// which is itself scoped and is not thread-safe.
    /// </remarks>
    public static IServiceCollection AddCoffeeLoyaltyServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
