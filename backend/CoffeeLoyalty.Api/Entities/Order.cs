using System.ComponentModel.DataAnnotations.Schema;
using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Entities;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }

    /// <summary>Full order value; always equals the sum of its lines (Σ Quantity × UnitPriceSnapshot). Cash paid is computed, never stored.</summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal Total { get; set; }

    /// <summary>Calculated on cash paid only, not on Total (decision 9).</summary>
    public int PointsEarned { get; set; } = 0;

    /// <summary>Points spent on this order; defaults to 0 and subject to the redemption constraints.</summary>
    public int PointsRedeemed { get; set; } = 0;

    // TODO: stored as string via HasConversion<string>() in Fluent API.
    public OrderStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<PointsTransaction> PointsTransactions { get; set; } = new List<PointsTransaction>();
}
