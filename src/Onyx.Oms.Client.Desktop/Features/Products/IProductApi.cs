using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public interface IProductApi
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

    [Get("/api/v1/products/{id}")]
    Task<ProductDetailsDto> GetProductById(Guid id);

    [Post("/api/v1/products")]
    Task<Guid> CreateProduct([Body] CreateProductCommand command);
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

public record ProductDetailsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BaseSku { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryPath { get; init; } = string.Empty;
    public decimal BaseCostAmount { get; init; }
    public string BaseCostCurrency { get; init; } = "LKR";
    public decimal BasePriceAmount { get; init; }
    public string BasePriceCurrency { get; init; } = "LKR";
    public decimal? BaseWeightAmount { get; init; }
    public string BaseWeightCurrency { get; init; } = "kg";
    public bool HasVariants { get; init; }
    public int StockOnHand { get; init; }
    public int ReservedQuantity { get; init; }
    
    public List<ProductOptionDetailsDto> Options { get; init; } = new();
    public List<ProductVariantDto> Variants { get; init; } = new();
    public List<ProductImageDto> Images { get; init; } = new();
    
    public List<ProductSpecificationDetailsDto> Specifications { get; init; } = new();
    
    public bool IsActive { get; init; }
}

public record ProductSpecificationDetailsDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public record ProductOptionDetailsDto
{
    public string Name { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    
    [System.Text.Json.Serialization.JsonPropertyName("values")]
    public List<string> Values { get; init; } = new();
}

public record ProductVariantDto
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public List<ProductVariantAttributeDto> Attributes { get; init; } = new();
    public decimal CostAmount { get; init; }
    public string CostCurrency { get; init; } = "LKR";
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; } = "LKR";
    public decimal? WeightAmount { get; init; }
    public string WeightCurrency { get; init; } = "kg";
    public int StockOnHand { get; init; }
    public int ReservedQuantity { get; init; }
    public bool IsActive { get; init; }
}

public record ProductVariantAttributeDto
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public record ProductImageDto
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public bool IsMain { get; init; }
    public string? OptionName { get; init; }
    public string? OptionValue { get; init; }
}
