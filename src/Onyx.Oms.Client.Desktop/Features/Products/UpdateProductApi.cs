using Refit;
using System;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public interface IUpdateProductApi
{
    [Put("/api/v1/products/{id}/basic-info")]
    Task UpdateBasicInformation(Guid id, [Body] UpdateProductBasicInfoCommand command);
    
    [Put("/api/v1/products/{id}/specifications")]
    Task UpdateSpecifications(Guid id, [Body] UpdateProductSpecificationsCommand command);

    [Put("/api/v1/products/{id}/base-logistics")]
    Task UpdateBaseLogistics(Guid id, [Body] UpdateProductBaseLogisticsCommand command);

    [Put("/api/v1/products/{id}/toggle-variants")]
    Task ToggleVariants(Guid id, [Body] ToggleProductVariantsCommand command);

    [Put("/api/v1/products/{id}/default-variant-logistics")]
    Task UpdateDefaultVariantLogistics(Guid id, [Body] UpdateDefaultVariantLogisticsCommand command);

    [Put("/api/v1/products/{id}/options")]
    Task UpdateOptions(Guid id, [Body] UpdateProductOptionsCommand command);

    [Put("/api/v1/products/{id}/variants")]
    Task UpdateVariants(Guid id, [Body] UpdateProductVariantsCommand command);
}

public record UpdateProductBasicInfoCommand(
    string Name,
    string? Description,
    string? BaseSku,
    Guid CategoryId,
    System.Collections.Generic.List<string> Tags
);

public record UpdateProductSpecificationsCommand(
    System.Collections.Generic.Dictionary<string, string> Specifications
);

public record UpdateProductBaseLogisticsCommand(
    UpdateMoneyDto BaseCost,
    UpdateMoneyDto BasePrice,
    UpdateWeightDto? BaseWeight
);

public record UpdateMoneyDto(decimal Amount, string Currency);

public record UpdateWeightDto(decimal Value, string Unit);

public record ToggleProductVariantsCommand(bool HasVariants);

public record UpdateDefaultVariantLogisticsCommand(
    Guid ProductId,
    string? Sku,
    UpdateMoneyDto Cost,
    UpdateMoneyDto Price,
    UpdateWeightDto? Weight,
    int StockOnHand
);

public record UpdateProductOptionsCommand(
    System.Collections.Generic.List<UpdateProductOptionDto> Options
);

public record UpdateProductOptionDto(
    string Name,
    int DisplayOrder,
    System.Collections.Generic.List<string> Values
);

public record UpdateProductVariantsCommand(
    System.Collections.Generic.List<UpdateProductVariantDto> Variants
);

public record UpdateProductVariantDto(
    Guid Id,
    string Sku,
    System.Collections.Generic.List<CreateVariantAttributeDto> Attributes,
    decimal CostAmount,
    decimal PriceAmount,
    decimal? WeightAmount,
    int StockOnHand,
    bool IsActive
);
