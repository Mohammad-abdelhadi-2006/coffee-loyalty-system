using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Entities;

public class Order
{
    /// <summary>
    /// Longest accepted <c>Idempotency-Key</c>, and the width of the column that stores it.
    /// A UUID or ULID fits with room to spare; anything longer is rejected by the service
    /// rather than silently truncated or blown up by SQL Server (same principle as decision 31).
    /// </summary>
    public const int IdempotencyKeyMaxLength = 64;

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

    /// <summary>Completed / Returned / Cancelled; stored as its member name, not its ordinal — see the HasConversion&lt;string&gt;() mapping in AppDbContext.OnModelCreating.</summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// The client's replay token for this creation (decision 40), NULL for an order created
    /// without an <c>Idempotency-Key</c> header. Unique among the orders that carry one,
    /// enforced by the filtered index <c>UX_Order_IdempotencyKey</c> — that index, not the
    /// service's up-front lookup, is what makes a retried submit land on one order.
    /// </summary>
    [MaxLength(IdempotencyKeyMaxLength)]
    public string? IdempotencyKey { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<PointsTransaction> PointsTransactions { get; set; } = new List<PointsTransaction>();
}
