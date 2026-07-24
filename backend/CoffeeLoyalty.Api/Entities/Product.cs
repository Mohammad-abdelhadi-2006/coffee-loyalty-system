using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Entities;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,3)")]
    public decimal Price { get; set; }

    // TODO: stored as string via HasConversion<string>() in Fluent API.
    /// <summary>Piece or Kg; determines whether OrderItem.Quantity is a count or a weight.</summary>
    public UnitType UnitType { get; set; }

    // TODO: stored as string via HasConversion<string>() in Fluent API.
    /// <summary>Menu section. Stored in English; the client maps to the
    /// display label.</summary>
    public ProductCategory Category { get; set; }

    /// <summary>Temporary out-of-stock toggle by the cashier (available today or not).</summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>Permanent on-the-menu flag (admin soft-delete); false hides it but keeps old OrderItems' FK.</summary>
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
