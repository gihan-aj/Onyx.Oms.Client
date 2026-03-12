using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public interface IProductsApi
{
    [Post("/api/v1/products")]
    Task<Guid> CreateProduct([Body] CreateProductDto dto);

    [Get("/api/v1/products/search")]
    Task<PagedResult<ProductDto>> GetProductsPaged(
        int page, 
        int pageSize, 
        [AliasAs("searchTerm")] string? searchTerm = null, 
        [AliasAs("sortColumn")] string? sortColumn = null,
        [AliasAs("sortOrder")] string? sortOrder = null,
        [AliasAs("isActive")] bool? isActive = null,
        [AliasAs("categoryId")] Guid? categoryId = null,
        [AliasAs("hasVariants")] bool? hasVariants = null);

    [Get("/api/v1/products/{id}")]
    Task<ProductDetailsDto> GetProductById(Guid id);

    [Put("/api/v1/products/{id}/basic-info")]
    Task UpdateProductBasicInfo(Guid id, [Body] UpdateProductBasicInfoDto dto);

    [Put("/api/v1/products/{id}/base-logistics")]
    Task UpdateProductBaseLogistics(Guid id, [Body] UpdateProductBaseLogisticsDto dto);

    [Put("/api/v1/products/{id}/specifications")]
    Task UpdateProductSpecifications(Guid id, [Body] UpdateProductSpecificationsDto dto);

    [Put("/api/v1/products/{id}/options")]
    Task UpdateProductOptions(Guid id, [Body] UpdateProductOptionsDto dto);

    [Put("/api/v1/products/{id}/toggle-variants")]
    Task ToggleProductVariants(Guid id, [Body] ToggleProductVariantsDto dto);

    [Put("/api/v1/products/{id}/default-variant-logistics")]
    Task UpdateDefaultVariantLogistics(Guid id, [Body] UpdateDefaultVariantLogisticsDto dto);

    [Put("/api/v1/products/{productId}/variants/{variantId}/logistics")]
    Task UpdateProductVariantLogistics(Guid productId, Guid variantId, [Body] UpdateProductVariantLogisticsDto dto);

    [Post("/api/v1/products/{productId}/variants")]
    Task AddProductVariant(Guid productId, [Body] AddProductVariantDto dto);

    [Delete("/api/v1/products/{productId}/variants/{variantId}")]
    Task DeleteProductVariant(Guid productId, Guid variantId);

    [Put("/api/v1/products/{id}/activate")]
    Task ActivateProduct(Guid id);

    [Put("/api/v1/products/{id}/deactivate")]
    Task DeactivateProduct(Guid id);
}