using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Constants;
using CoffeeLoyalty.Api.Data;
using CoffeeLoyalty.Api.Dtos.Employees;
using CoffeeLoyalty.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// All staff-account rules live here; the controller only maps HTTP to these calls.
/// </summary>
public sealed class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;
    private readonly ILogger<EmployeeService> _logger;

    /// <summary>
    /// Creates the service.
    /// </summary>
    /// <param name="db">The application database context.</param>
    /// <param name="logger">Diagnostics; never surfaced to the caller.</param>
    public EmployeeService(AppDbContext db, ILogger<EmployeeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EmployeeResponse>> GetEmployeesAsync(CancellationToken cancellationToken = default)
    {
        // Projected in SQL: the password hashes are never loaded into memory at all, so
        // there is no object in this process that could be serialized by accident.
        var employees = await _db.Employees
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Select(e => new
            {
                e.Id,
                e.FullName,
                e.Username,
                e.Role,
                e.IsActive,
                e.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return employees
            .Select(e => new EmployeeResponse
            {
                Id = e.Id,
                FullName = e.FullName,
                Username = e.Username,
                Role = RoleNames.ToWireString(e.Role),
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt
            })
            .ToList();
    }

    /// <inheritdoc />
    public async Task<EmployeeResponse> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!RoleNames.TryParseWireString(request.Role, out var role))
        {
            throw ApiException.ValidationError("الصلاحية غير صالحة، القيم المسموحة: cashier أو admin");
        }

        var fullName = NormalizeName(request.FullName);
        var username = NormalizeUsername(request.Username);

        if (await _db.Employees.AnyAsync(e => e.Username == username, cancellationToken))
        {
            throw UsernameTaken();
        }

        var employee = new Employee
        {
            FullName = fullName,
            Username = username,
            // The same call the admin seeder makes, at the library's default work factor —
            // so a seeded hash and an API-created one are verified identically, and the
            // login path's dummy hash keeps costing what a real one costs.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Employees.Add(employee);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // The check above is not a lock: two admins creating the same username at once
            // both pass it, and the unique index rejects the loser. That loser must still
            // be told the username is taken rather than get a 500 — but only once the row
            // is confirmed to exist, so an unrelated write failure is not mislabelled.
            _db.Entry(employee).State = EntityState.Detached;

            if (!await _db.Employees.AnyAsync(e => e.Username == username, cancellationToken))
            {
                throw;
            }

            _logger.LogWarning(ex, "Concurrent creation of the same username; the later one is rejected.");

            throw UsernameTaken();
        }

        return ToResponse(employee);
    }

    /// <inheritdoc />
    public async Task<EmployeeResponse> SetStatusAsync(
        int id,
        UpdateEmployeeStatusRequest request,
        int currentEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var isActive = request.IsActive!.Value;

        // Checked before the row is even read: an admin who locks themselves out cannot
        // undo it, because undoing it is an admin-only endpoint. Only deactivation is
        // refused — an admin re-activating their own already-active account is harmless.
        if (id == currentEmployeeId && !isActive)
        {
            throw ApiException.ValidationError("لا يمكنك تعطيل حسابك الخاص");
        }

        var employee = await _db.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (employee is null)
        {
            throw ApiException.EmployeeNotFound();
        }

        employee.IsActive = isActive;

        if (!isActive)
        {
            // Decision 25/18: deactivation takes effect now, not when the token expires.
            // Bumping the version invalidates every token already issued to this account.
            employee.TokenVersion++;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return ToResponse(employee);
    }

    /// <summary>
    /// The username-taken failure. The contract routes it through
    /// <c>VALIDATION_ERROR</c> rather than a conflict code (§2), so the message has to
    /// carry the meaning by itself.
    /// </summary>
    /// <returns>The exception to throw.</returns>
    private static ApiException UsernameTaken() =>
        ApiException.ValidationError("اسم المستخدم مستخدم مسبقاً");

    /// <summary>
    /// Trims the name and refuses one that is nothing but whitespace —
    /// <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> accepts <c>" "</c>.
    /// </summary>
    /// <param name="fullName">The submitted name.</param>
    /// <returns>The trimmed name.</returns>
    /// <exception cref="ApiException">400 <c>VALIDATION_ERROR</c> when nothing is left after trimming.</exception>
    private static string NormalizeName(string fullName)
    {
        var trimmed = fullName.Trim();

        return trimmed.Length > 0
            ? trimmed
            : throw ApiException.ValidationError("اسم الموظف مطلوب");
    }

    /// <summary>
    /// Trims the login name. Case is left exactly as the admin typed it: the column's
    /// collation is case-insensitive, so the unique index already prevents
    /// <c>ahmad</c> and <c>Ahmad</c> from both existing, and the login path compares the
    /// same way — folding the case here would only make the stored name differ from the
    /// one the admin was shown.
    /// </summary>
    /// <param name="username">The submitted login name.</param>
    /// <returns>The trimmed username.</returns>
    /// <exception cref="ApiException">400 <c>VALIDATION_ERROR</c> when nothing is left after trimming.</exception>
    private static string NormalizeUsername(string username)
    {
        var trimmed = username.Trim();

        return trimmed.Length > 0
            ? trimmed
            : throw ApiException.ValidationError("اسم المستخدم مطلوب");
    }

    /// <summary>
    /// Maps an employee row to the contract's employee object.
    /// </summary>
    /// <param name="employee">The entity.</param>
    /// <returns>The response DTO, without the hash or the token version.</returns>
    private static EmployeeResponse ToResponse(Employee employee) => new()
    {
        Id = employee.Id,
        FullName = employee.FullName,
        Username = employee.Username,
        Role = RoleNames.ToWireString(employee.Role),
        IsActive = employee.IsActive,
        CreatedAt = employee.CreatedAt
    };
}
