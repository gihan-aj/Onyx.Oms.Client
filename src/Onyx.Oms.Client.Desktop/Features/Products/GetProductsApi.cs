using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public interface IGetProductsApi
{
    [Get("/api/v1/products/search")]
    Task<PagedResult<ProductDto>> SearchProducts(
        [AliasAs("Page")] int page, 
        [AliasAs("PageSize")] int pageSize, 
        [AliasAs("SearchTerm")] string? searchTerm = null,
        [AliasAs("SortColumn")] string? sortColumn = null, 
        [AliasAs("SortOrder")] string? sortOrder = null,
        [AliasAs("IsActive")] bool? isActive = null,
        [AliasAs("CategoryId")] Guid? categoryId = null,
        [AliasAs("HasVariants")] bool? hasVariants = null);

    [Put("/api/v1/products/{id}/activate")]
    Task ActivateProduct(Guid id);

    [Put("/api/v1/products/{id}/deactivate")]
    Task DeactivateProduct(Guid id);
}

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BaseSku { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CategoryPath { get; init; }
    public decimal BasePriceAmount { get; init; }
    public string BasePriceCurrency { get; init; } = string.Empty;
    public string? MainImageUrl { get; init; }
    public bool HasVariants { get; init; }
    public string HasVariantsText => HasVariants ? "Yes" : "No";
    public int StockOnHand { get; init; }
    public int AvailableQuantity { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedOnUtc { get; init; }
    public DateTime? LastModifiedOnUtc { get; init; }

    // UI Helper properties that are hydrated post-fetch
    public bool CanEdit { get; set; }
    public bool CanActivate { get; set; }
    public bool CanDeactivate { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Microsoft.UI.Xaml.Media.Imaging.BitmapImage? MainImageSource { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsOutOfStock => AvailableQuantity <= 0;

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsLowStock => AvailableQuantity > 0 && AvailableQuantity <= 10;

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsInStock => AvailableQuantity > 10;
}
