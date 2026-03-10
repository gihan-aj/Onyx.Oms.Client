using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public interface ICreateProductApi
{
    [Post("/api/v1/products")]
    Task<Guid> CreateProduct([Body] CreateProductCommand command);
}

public record CreateProductCommand(
    string Name,
    string? BaseSku,
    string? Description,
    Guid CategoryId,
    CreateMoneyDto BaseCost,
    CreateMoneyDto BasePrice,
    CreateWeightDto? BaseWeight,
    bool HasVariants,
    int? BaseStockOnHand,
    List<CreateProductOptionDto>? Options,
    Dictionary<string, string>? Specifications,
    List<CreateProductVariantDto>? Variants,
    List<CreateProductImageDto>? Images,
    List<string>? Tags
);

public record CreateMoneyDto(decimal Amount, string Currency);

public record CreateWeightDto(decimal Value, string Unit);

public record CreateProductOptionDto(
    string Name,
    List<string> Values
);

public record CreateVariantAttributeDto(
    string Name,
    string Value
);

public record CreateProductVariantDto(
    string? Sku,
    List<CreateVariantAttributeDto> Attributes,
    CreateMoneyDto? Cost,
    CreateMoneyDto? Price,
    CreateWeightDto? Weight,
    int StockOnHand
);

public record CreateProductImageDto(
    string Url,
    int DisplayOrder,
    bool IsMain,
    string? OptionName,
    string? OptionValue
);
