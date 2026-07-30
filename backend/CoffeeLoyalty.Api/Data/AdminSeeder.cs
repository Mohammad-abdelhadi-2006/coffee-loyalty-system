using CoffeeLoyalty.Api.Entities;
using CoffeeLoyalty.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoffeeLoyalty.Api.Data;

/// <summary>
/// Seeds the single admin account the dashboard is bootstrapped with (decision 4:
/// no self-registration). The credentials come from User Secrets in development and
/// from environment variables in production — never from a committed file — and the
/// seeder refuses to start the app if they are absent rather than falling back to a
/// default password nobody changes.
/// </summary>
public static class AdminSeeder
{
    /// <summary>Configuration key for the admin's login name.</summary>
    public const string UsernameKey = "Seed:AdminUsername";

    /// <summary>Configuration key for the admin's initial password (sensitive).</summary>
    public const string PasswordKey = "Seed:AdminPassword";

    /// <summary>Configuration key for the admin's display name (not sensitive).</summary>
    public const string FullNameKey = "Seed:AdminFullName";

    /// <summary>
    /// Creates the admin employee if no row with that username exists. Idempotent:
    /// a second run neither duplicates the account nor resets an already-changed password.
    /// </summary>
    /// <param name="services">A scoped service provider (the seeder resolves its own DbContext).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">The seed username or password is missing.</exception>
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AdminSeeder));

        var username = configuration[UsernameKey];
        var password = configuration[PasswordKey];

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException(
                $"{UsernameKey} is not configured. Set it with: dotnet user-secrets set \"{UsernameKey}\" \"<username>\"");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"{PasswordKey} is not configured. Set it with: dotnet user-secrets set \"{PasswordKey}\" \"<strong password>\". " +
                "The application will not start with a default password.");
        }

        var db = services.GetRequiredService<AppDbContext>();

        if (await db.Employees.AnyAsync(e => e.Username == username, cancellationToken))
        {
            logger.LogInformation("Admin account '{Username}' already exists; seeding skipped.", username);
            return;
        }

        db.Employees.Add(new Employee
        {
            FullName = configuration[FullNameKey] is { Length: > 0 } fullName ? fullName : username,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = EmployeeRole.Admin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded the admin account '{Username}'.", username);
    }
}
