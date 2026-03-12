using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public interface IProductCategoryLookupApi
{
    [Get("/api/v1/product-categories/search")]
    Task<PagedResult<ProductCategoryDto>> SearchCategories(
        [AliasAs("Page")] int page,
        [AliasAs("PageSize")] int pageSize,
        [AliasAs("SearchTerm")] string? searchTerm = null,
        [AliasAs("SortColumn")] string? sortColumn = null,
        [AliasAs("SortOrder")] string? sortOrder = null,
        [AliasAs("IsActive")] bool? isActive = null,
        [AliasAs("IsValidParent")] bool? isValidParent = null,
        [AliasAs("IsLeafOnly")] bool? isLeafOnly = null);

    [Get("/api/v1/product-categories/{id}")]
    Task<ProductCategoryResponse> GetCategoryById(Guid id, [AliasAs("includeParentSpecs")] bool includeParentSpecs = true);
}