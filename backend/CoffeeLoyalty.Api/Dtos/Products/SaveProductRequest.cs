using System.ComponentModel.DataAnnotations;
using CoffeeLoyalty.Api.Enums;

namespace CoffeeLoyalty.Api.Dtos.Products;

/// <summary>
/// Body of <c>POST /api/products</c> and <c>PUT /api/products/{id}</c> — the contract
/// gives both the same shape, so they share one DTO rather than two identical ones.
/// </summary>
/// <remarks>
/// The two enums are declared nullable on purpose. A non-nullable enum property has no
/// "absent" state — an omitted <c>category</c> would silently bind to member 0
/// (<c>HotCoffee</c>) and be saved as if the admin had chosen it. Nullable plus
/// <see cref="RequiredAttribute"/> makes the omission a <c>VALIDATION_ERROR</c>, and an
/// unrecognized string is rejected by the enum converter during binding, which lands in
/// the same place.
/// </remarks>
public class SaveProductRequest
{
    /// <summary>Display name. Capped at the column width on Product.</summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Price in JOD, strictly positive. The upper bound is the largest value the
    /// <c>decimal(18,3)</c> column can hold — without it an oversized price would pass
    /// validation and then fail in SQL Server as an unhandled 500.
    /// </summary>
    [Required]
    [Range(typeof(decimal), "0.001", "999999999999999.999",
        ConvertValueInInvariantCulture = true,
        ParseLimitsInInvariantCulture = true)]
    public decimal? Price { get; set; }

    /// <summary><c>Piece</c> or <c>Kg</c>.</summary>
    [Required]
    public Enums.UnitType? UnitType { get; set; }

    /// <summary>One of the six menu sections in <see cref="ProductCategory"/>.</summary>
    [Required]
    public ProductCategory? Category { get; set; }
}
