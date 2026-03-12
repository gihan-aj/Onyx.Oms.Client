using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public class MoneyDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "LKR";
}

public class WeightDto
{
    public decimal Value { get; set; }
    public string Unit { get; set; } = "kg";
}

public class ProductOptionDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new();
}

public class ProductVariantAttributeDto
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class CreateProductImageDto
{
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsMain { get; set; }
    public string? OptionName { get; set; }
    public string? OptionValue { get; set; }
}

public class CreateProductVariantDto
{
    public string? Sku { get; set; }
    public List<ProductVariantAttributeDto> Attributes { get; set; } = new();
    public MoneyDto Cost { get; set; } = new();
    public MoneyDto Price { get; set; } = new();
    public WeightDto Weight { get; set; } = new();
    public int StockOnHand { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? BaseSku { get; set; }
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public MoneyDto BaseCost { get; set; } = new();
    public MoneyDto BasePrice { get; set; } = new();
    public WeightDto BaseWeight { get; set; } = new();
    public int? BaseStockOnHand { get; set; }
    public List<ProductOptionDto> Options { get; set; } = new();
    public Dictionary<string, string> Specifications { get; set; } = new();
    public List<CreateProductVariantDto> Variants { get; set; } = new();
    public List<CreateProductImageDto> Images { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseSku { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryPath {  get; set; } = string.Empty;
    public decimal BasePriceAmount { get; set; }
    public string BasePriceCurrency { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public bool HasVariants { get; set; }
    public int StockOnHand { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? LastModifiedOnUtc { get; set; }
}

public class ProductSpecificationDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ProductDetailsOptionDto
{
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<string> Values { get; set; } = new();
}

public class ProductDetailsVariantDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public List<ProductVariantAttributeDto> Attributes { get; set; } = new();
    public decimal CostAmount { get; set; }
    public string CostCurrency { get; set; } = "LKR";
    public decimal PriceAmount { get; set; }
    public string PriceCurrency { get; set; } = "LKR";
    public decimal WeightAmount { get; set; }
    public string WeightCurrency { get; set; } = "kg";
    public int StockOnHand { get; set; }
    public int ReservedQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class ProductDetailsImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsMain { get; set; }
    public string? OptionName { get; set; }
    public string? OptionValue { get; set; }
}

public class ProductDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseSku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryPath { get; set; } = string.Empty;
    public List<ProductSpecificationDto> Specifications { get; set; } = new();
    public decimal BaseCostAmount { get; set; }
    public string BaseCostCurrency { get; set; } = string.Empty;
    public decimal BasePriceAmount { get; set; }
    public string BasePriceCurrency { get; set; } = string.Empty;
    public decimal BaseWeightAmount { get; set; }
    public string BaseWeightCurrency { get; set; } = string.Empty;
    public bool HasVariants { get; set; }
    public int StockOnHand { get; set; }
    public int ReservedQuantity { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<ProductDetailsOptionDto> Options { get; set; } = new();
    public List<ProductDetailsVariantDto> Variants { get; set; } = new();
    public List<ProductDetailsImageDto> Images { get; set; } = new();
    public bool IsActive { get; set; }
}

public class UpdateProductBasicInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BaseSku { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class UpdateProductBaseLogisticsDto
{
    public Guid Id { get; set; }
    public MoneyDto BaseCost { get; set; } = new();
    public MoneyDto BasePrice { get; set; } = new();
    public WeightDto BaseWeight { get; set; } = new();
}

public class UpdateProductSpecificationsDto
{
    public Guid Id { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new();
}

public class UpdateProductOptionsDto
{
    public Guid Id { get; set; }
    public List<ProductOptionDto> Options { get; set; } = new();
}

public class ToggleProductVariantsDto
{
    public Guid Id { get; set; }
    public bool HasVariants { get; set; }
}

public class UpdateDefaultVariantLogisticsDto
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public MoneyDto Cost { get; set; } = new();
    public MoneyDto Price { get; set; } = new();
    public WeightDto Weight { get; set; } = new();
    public int StockOnHand { get; set; }
}

public class UpdateProductVariantLogisticsDto
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public MoneyDto Cost { get; set; } = new();
    public MoneyDto Price { get; set; } = new();
    public WeightDto Weight { get; set; } = new();
    public int StockOnHand { get; set; }
}

public class AddProductVariantDto
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public List<ProductVariantAttributeDto> Attributes { get; set; } = new();
    public MoneyDto Cost { get; set; } = new();
    public MoneyDto Price { get; set; } = new();
    public WeightDto Weight { get; set; } = new();
    public int StockOnHand { get; set; }
}
