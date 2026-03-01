using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public record CreateProductCommand(
    string Name,
    string? BaseSku,
    string? Description,
    Guid CategoryId,
    string? Brand,
    string? Material,
    int Gender,
    decimal BaseCostAmount,
    string BaseCostCurrency,
    decimal BasePriceAmount,
    string BasePriceCurrency,
    decimal BaseWeightValue,
    string BaseWeightUnit,
    bool HasColor,
    bool HasSize,
    List<string>? Tags,
    List<CreateProductVariantDto> Variants,
    List<CreateProductImageDto> Images
);

public record CreateProductVariantDto(
    string? Sku,
    string? Color,
    string? Size,
    decimal? CostAmount,
    decimal? PriceAmount,
    decimal? WeightValue,
    int StockOnHand
);

public record CreateProductImageDto(
    string Url,
    int DisplayOrder,
    bool IsMain,
    string? Color
);
