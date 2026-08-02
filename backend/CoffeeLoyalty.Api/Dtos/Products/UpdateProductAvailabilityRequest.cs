using System.ComponentModel.DataAnnotations;

namespace CoffeeLoyalty.Api.Dtos.Products;

/// <summary>
/// Body of <c>PATCH /api/products/{id}/availability</c> — the cashier's
/// "out of stock today" toggle (decision 14).
/// </summary>
public class UpdateProductAvailabilityRequest
{
    /// <summary>
    /// The new availability. Nullable so that an omitted field is a
    /// <c>VALIDATION_ERROR</c> rather than a silent <c>false</c> that takes the
    /// product off the menu nobody asked to change.
    /// </summary>
    [Required]
    public bool? IsAvailable { get; set; }
}
