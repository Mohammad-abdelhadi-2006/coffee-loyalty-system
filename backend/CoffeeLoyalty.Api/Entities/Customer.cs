using System.ComponentModel.DataAnnotations;

namespace CoffeeLoyalty.Api.Entities;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>The customer's identity in the system; a customer without one does not exist. Stored in E.164 format.</summary>
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Nullable: cashier-registered customers have no Firebase account until their first app login, then it is linked by PhoneNumber.</summary>
    [MaxLength(128)]
    public string? FirebaseUid { get; set; }

    /// <summary>Denormalized running balance (perf decision); guaranteed >= 0 by a DB CHECK constraint.</summary>
    public int PointsBalance { get; set; } = 0;

    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<PointsTransaction> PointsTransactions { get; set; } = new List<PointsTransaction>();
}
