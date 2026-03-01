using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
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
        [AliasAs("CategoryId")] Guid? categoryId = null);

    [Put("/api/v1/products/{id}/activate")]
    Task ActivateProduct(Guid id);

    [Put("/api/v1/products/{id}/deactivate")]
    Task DeactivateProduct(Guid id);

    [Post("/api/v1/products")]
    Task<Guid> CreateProduct([Body] CreateProductCommand command);
}
