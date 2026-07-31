using CoffeeLoyalty.Api.Dtos.Products;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// The product catalog (api-contract.md §3): what the app and the counter can see,
/// and the two levels of removal — the cashier's temporary one and the admin's permanent one.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Lists the catalog. A soft-deleted product is never returned to anybody.
    /// </summary>
    /// <param name="customerView">
    /// <c>true</c> for the app menu — unavailable products are hidden as well, because a
    /// customer cannot be shown something the counter cannot make. <c>false</c> for staff,
    /// who need to see the unavailable rows in order to switch them back on.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The visible products, in insertion order.</returns>
    Task<IReadOnlyList<ProductResponse>> GetProductsAsync(bool customerView, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a product to the menu, available and active.
    /// </summary>
    /// <param name="request">Name, price, unit type and category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created product.</returns>
    /// <exception cref="Common.ApiException">400 <c>VALIDATION_ERROR</c> for a blank name or a price finer than the column's three decimals.</exception>
    Task<ProductResponse> CreateAsync(SaveProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Overwrites a product's catalog fields. Availability is not part of this body —
    /// it belongs to the cashier's toggle, not to the admin's edit form.
    /// </summary>
    /// <param name="id">The product's id.</param>
    /// <param name="request">The new name, price, unit type and category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated product.</returns>
    /// <remarks>
    /// A price change is a change to the catalog only: every OrderItem already carries its
    /// own <c>UnitPriceSnapshot</c>, so no historical total can move (decision 3).
    /// </remarks>
    /// <exception cref="Common.ApiException">404 <c>PRODUCT_NOT_FOUND</c>; 400 <c>VALIDATION_ERROR</c>.</exception>
    Task<ProductResponse> UpdateAsync(int id, SaveProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a product by clearing <c>IsActive</c> (decision 14). The row survives
    /// so that the OrderItems pointing at it keep resolving.
    /// </summary>
    /// <param name="id">The product's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="Common.ApiException">404 <c>PRODUCT_NOT_FOUND</c>.</exception>
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Flips the "in stock today" toggle (decision 14).
    /// </summary>
    /// <param name="id">The product's id.</param>
    /// <param name="request">The new availability.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated product.</returns>
    /// <exception cref="Common.ApiException">404 <c>PRODUCT_NOT_FOUND</c>.</exception>
    Task<ProductResponse> SetAvailabilityAsync(int id, UpdateProductAvailabilityRequest request, CancellationToken cancellationToken = default);
}
