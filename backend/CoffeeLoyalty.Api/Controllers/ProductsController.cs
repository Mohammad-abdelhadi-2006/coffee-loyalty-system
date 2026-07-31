using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Constants;
using CoffeeLoyalty.Api.Dtos.Products;
using CoffeeLoyalty.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeLoyalty.Api.Controllers;

/// <summary>
/// The product catalog (api-contract.md §3). Thin — every rule lives in
/// <see cref="IProductService"/>.
/// </summary>
/// <remarks>
/// Three different auth levels meet on this route: the menu is readable by everyone
/// signed in, availability is the cashier's, and the catalog itself is the admin's. Each
/// action therefore carries its own attribute; there is no controller-wide one to inherit.
/// </remarks>
[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;

    /// <summary>
    /// Creates the controller.
    /// </summary>
    /// <param name="products">The catalog service.</param>
    public ProductsController(IProductService products)
    {
        _products = products;
    }

    /// <summary>
    /// Lists the catalog, filtered by who is asking.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The app menu for a customer; every active product for staff.</returns>
    /// <remarks>
    /// The only endpoint open to all three roles at once. Policies combine with AND when
    /// stacked, so the union is expressed with <c>Roles</c> against the same
    /// <see cref="RoleNames"/> constants the policies are built from — listing the roles
    /// explicitly rather than accepting any authenticated principal, so that a future
    /// fourth role has to be let in on purpose.
    /// </remarks>
    /// <response code="200">The visible products.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    [HttpGet]
    [Authorize(Roles = $"{RoleNames.Customer},{RoleNames.Cashier},{RoleNames.Admin}")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await _products.GetProductsAsync(User.IsInRole(RoleNames.Customer), cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Adds a product to the menu.
    /// </summary>
    /// <param name="request">Name, price, unit type and category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created product.</returns>
    /// <response code="201">Created; the body is the new product.</response>
    /// <response code="400">VALIDATION_ERROR — malformed body, or an unknown unit type or category.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not an admin.</response>
    [HttpPost]
    [Authorize(Policy = RoleNames.Admin)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody] SaveProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _products.CreateAsync(request, cancellationToken);

        // 201 without a Location header: the contract defines no GET /api/products/{id},
        // so there is no URI to point the client at.
        return StatusCode(StatusCodes.Status201Created, product);
    }

    /// <summary>
    /// Overwrites a product's catalog fields.
    /// </summary>
    /// <param name="id">The product's id.</param>
    /// <param name="request">The new name, price, unit type and category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated product.</returns>
    /// <response code="200">Updated.</response>
    /// <response code="400">VALIDATION_ERROR — malformed body, or an unknown unit type or category.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not an admin.</response>
    /// <response code="404">PRODUCT_NOT_FOUND.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = RoleNames.Admin)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(
        int id,
        [FromBody] SaveProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _products.UpdateAsync(id, request, cancellationToken);
        return Ok(product);
    }

    /// <summary>
    /// Takes a product off the menu for good (soft delete, decision 14).
    /// </summary>
    /// <param name="id">The product's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Deactivated; empty body.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not an admin.</response>
    /// <response code="404">PRODUCT_NOT_FOUND.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        await _products.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Flips the "in stock today" toggle.
    /// </summary>
    /// <param name="id">The product's id.</param>
    /// <param name="request">The new availability.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated product.</returns>
    /// <response code="200">Updated.</response>
    /// <response code="400">VALIDATION_ERROR — malformed body.</response>
    /// <response code="401">UNAUTHORIZED — missing or rejected token.</response>
    /// <response code="403">FORBIDDEN — not a cashier or admin.</response>
    /// <response code="404">PRODUCT_NOT_FOUND.</response>
    [HttpPatch("{id:int}/availability")]
    [Authorize(Policy = RoleNames.Cashier)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> SetAvailability(
        int id,
        [FromBody] UpdateProductAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _products.SetAvailabilityAsync(id, request, cancellationToken);
        return Ok(product);
    }
}
