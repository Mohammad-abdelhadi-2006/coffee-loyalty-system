using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Dtos.Products;

/// <summary>
/// The one product shape every endpoint in api-contract.md §3 returns — the list,
/// the create, the update and the availability toggle all speak this.
/// </summary>
/// <remarks>
/// <see cref="UnitType"/> and <see cref="Category"/> go out as their enum member
/// names (<c>"Piece"</c>, <c>"HotCoffee"</c>), which is what the global
/// <c>JsonStringEnumConverter</c> registered in Program.cs produces. That is also
/// exactly how they are stored, so the wire value and the database value never drift.
/// </remarks>
public class ProductResponse
{
    /// <summary>Primary key; the id every other product route takes.</summary>
    public int Id { get; set; }

    /// <summary>Display name, as the admin typed it.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Current catalog price in JOD. Old orders keep their own snapshot (decision 3).</summary>
    public decimal Price { get; set; }

    /// <summary>Whether a quantity of this product is a count or a weight.</summary>
    public Enums.UnitType UnitType { get; set; }

    /// <summary>Menu section.</summary>
    public ProductCategory Category { get; set; }

    /// <summary>The cashier's "in stock today" toggle.</summary>
    public bool IsAvailable { get; set; }

    /// <summary>The admin's on-the-menu flag. Always <c>true</c> in responses — a
    /// soft-deleted product is never returned — but sent because the contract lists it.</summary>
    public bool IsActive { get; set; }
}
