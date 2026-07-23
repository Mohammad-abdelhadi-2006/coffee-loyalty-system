using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeLoyalty.Api.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public int ProductId { get; set; }

    /// <summary>Product name frozen at order time — like UnitPriceSnapshot, it preserves what was actually sold so a later product rename never rewrites past receipts (Decision 3).</summary>
    [Required]
    [MaxLength(100)]
    public string ProductNameSnapshot { get; set; } = string.Empty;

    /// <summary>Decimal to support both weight (1.5 kg) and count (2 pieces).</summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; }

    /// <summary>Defaults to 0; supports partial returns (0 ≤ ReturnedQuantity ≤ Quantity).</summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal ReturnedQuantity { get; set; } = 0;

    /// <summary>Unit price frozen at order time; unaffected by later catalog price changes.</summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal UnitPriceSnapshot { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
