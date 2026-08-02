using System.ComponentModel.DataAnnotations;

namespace CoffeeLoyalty.Api.Dtos.Customers;

/// <summary>
/// Body of <c>POST /api/customers</c> — the cashier registering a walk-in customer
/// at the counter (api-contract.md §4).
/// </summary>
public class CreateCustomerRequest
{
    /// <summary>The customer's display name. Capped at the column width on Customer.</summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// The number as the cashier typed it — <c>07XXXXXXXX</c>, <c>+9627XXXXXXXX</c>, with
    /// or without separators. The server normalizes it to E.164 before anything is stored,
    /// so the length limit is generous enough for the longest accepted spelling rather
    /// than the width of the column the canonical form ends up in.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
}
