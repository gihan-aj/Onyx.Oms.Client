using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public interface IGetProductDetailsApi
{
    [Get("/api/v1/products/{id}")]
    Task<ProductDetailsDto> GetProductById(Guid id);
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

    public List<string> Tags { get; init; } = new();
    public List<ProductOptionDetailsDto> Options { get; init; } = new();
    public List<ProductVariantDto> Variants { get; init; } = new();
    public List<ProductImageDto> Images { get; init; } = new();
    
    public List<ProductSpecificationDto> Specifications { get; init; } = new();
    
    public bool IsActive { get; init; }
}

public record ProductSpecificationDto
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
