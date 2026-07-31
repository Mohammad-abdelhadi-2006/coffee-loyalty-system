using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Data;
using CoffeeLoyalty.Api.Dtos.Products;
using CoffeeLoyalty.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeLoyalty.Api.Services;

/// <summary>
/// All catalog rules live here; the controller only maps HTTP to these calls.
/// </summary>
public sealed class ProductService : IProductService
{
    /// <summary>
    /// Decimal places the <c>Price</c> column keeps — <c>decimal(18,3)</c> on Product.
    /// A finer price is rejected rather than stored, because SQL Server would round it
    /// silently and the admin would be told a price they are not actually charging.
    /// </summary>
    private const int PriceScale = 3;

    private readonly AppDbContext _db;

    /// <summary>
    /// Creates the service.
    /// </summary>
    /// <param name="db">The application database context.</param>
    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductResponse>> GetProductsAsync(
        bool customerView,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (customerView)
        {
            query = query.Where(p => p.IsAvailable);
        }

        // Ordered by id — insertion order. The contract defines no menu order, and both
        // clients group by category themselves; an explicit order is here only so the
        // list does not reshuffle between two identical requests.
        var products = await query
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

        return products.Select(ToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<ProductResponse> CreateAsync(
        SaveProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = NormalizeName(request.Name),
            Price = ValidatePrice(request.Price!.Value),
            // Non-null by the time the action body runs: [Required] on the nullable enums
            // means model validation has already rejected an absent or unknown value.
            UnitType = request.UnitType!.Value,
            Category = request.Category!.Value,
            IsAvailable = true,
            IsActive = true
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    /// <inheritdoc />
    public async Task<ProductResponse> UpdateAsync(
        int id,
        SaveProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await FindActiveAsync(id, cancellationToken);

        product.Name = NormalizeName(request.Name);
        product.Price = ValidatePrice(request.Price!.Value);
        product.UnitType = request.UnitType!.Value;
        product.Category = request.Category!.Value;

        await _db.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    /// <inheritdoc />
    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await FindActiveAsync(id, cancellationToken);

        product.IsActive = false;

        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProductResponse> SetAvailabilityAsync(
        int id,
        UpdateProductAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await FindActiveAsync(id, cancellationToken);

        product.IsAvailable = request.IsAvailable!.Value;

        await _db.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    /// <summary>
    /// Loads a product for writing, or fails with the contract's 404.
    /// </summary>
    /// <param name="id">The product's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tracked entity.</returns>
    /// <remarks>
    /// Soft-deleted rows are excluded, so every write route treats them as gone — which is
    /// what the registry means by "no <i>active</i> product with that id". Consequently
    /// nothing here can bring a deleted product back; the contract has no reactivate route,
    /// and inventing one is the Products module's decision to take, not this method's.
    /// </remarks>
    /// <exception cref="ApiException">404 <c>PRODUCT_NOT_FOUND</c>.</exception>
    private async Task<Product> FindActiveAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

        return product ?? throw ApiException.ProductNotFound();
    }

    /// <summary>
    /// Trims the name and refuses one that is nothing but whitespace —
    /// <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> accepts <c>" "</c>.
    /// </summary>
    /// <param name="name">The submitted name.</param>
    /// <returns>The trimmed name.</returns>
    /// <exception cref="ApiException">400 <c>VALIDATION_ERROR</c> when nothing is left after trimming.</exception>
    private static string NormalizeName(string name)
    {
        var trimmed = name.Trim();

        return trimmed.Length > 0
            ? trimmed
            : throw ApiException.ValidationError("اسم المنتج مطلوب");
    }

    /// <summary>
    /// Rejects a price the price column cannot represent exactly. The lower bound is
    /// already enforced by the DTO's <c>Range</c>; only the scale is left to check.
    /// </summary>
    /// <param name="price">The submitted price.</param>
    /// <returns>The same price, once it is known to be storable.</returns>
    /// <exception cref="ApiException">400 <c>VALIDATION_ERROR</c> for more than three decimals.</exception>
    private static decimal ValidatePrice(decimal price) =>
        decimal.Round(price, PriceScale) == price
            ? price
            : throw ApiException.ValidationError("السعر يقبل ثلاث خانات عشرية كحد أقصى");

    /// <summary>
    /// Maps a product row to the contract's product object.
    /// </summary>
    /// <param name="product">The entity.</param>
    /// <returns>The response DTO.</returns>
    private static ProductResponse ToResponse(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        UnitType = product.UnitType,
        Category = product.Category,
        IsAvailable = product.IsAvailable,
        IsActive = product.IsActive
    };
}
