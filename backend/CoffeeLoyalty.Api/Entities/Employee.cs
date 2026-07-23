using System.ComponentModel.DataAnnotations;
using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Entities;

public class Employee
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    // TODO: stored as string via HasConversion<string>() in Fluent API.
    public EmployeeRole Role { get; set; }

    /// <summary>Soft-delete flag; deactivated (resigned/terminated) employees are rejected at login but kept for old Orders' FK.</summary>
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
