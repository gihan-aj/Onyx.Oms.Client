using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public record CreateProductCommand(
    string Name,
    string? BaseSku,
    string? Description,
    Guid CategoryId,
    MoneyDto BaseCost,
    MoneyDto BasePrice,
    WeightDto? BaseWeight,
    bool HasVariants,
    int? BaseStockOnHand,
    List<ProductOptionDto>? Options,
    Dictionary<string, string>? Specifications,
    List<CreateProductVariantDto>? Variants,
    List<CreateProductImageDto>? Images,
    List<string>? Tags
);

public record MoneyDto(decimal Amount, string Currency);

public record WeightDto(decimal Value, string Unit);

public record ProductOptionDto(
    string Name,
    List<string> Values
);

public record VariantAttributeDto(
    string Name,
    string Value
);

public record CreateProductVariantDto(
    string? Sku,
    List<VariantAttributeDto> Attributes,
    MoneyDto? Cost,
    MoneyDto? Price,
    WeightDto? Weight,
    int StockOnHand
);

public record CreateProductImageDto(
    string Url,
    int DisplayOrder,
    bool IsMain,
    string? OptionName,
    string? OptionValue
);
