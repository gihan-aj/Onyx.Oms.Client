using Onyx.Oms.Client.Desktop.Features.Products;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks
{
    public interface IFulfillmentTasksApi
    {
        [Get("/api/v1/fulfillment-tasks/search")]
        Task<PagedResult<FulfillmentTaskDto>> GetFulfillmentTasksPaged(
            int page,
            int pageSize,
            [AliasAs("searchTerm")] string? searchTerm = null,
            [AliasAs("sortColumn")] string? sortColumn = null,
            [AliasAs("sortOrder")] string? sortOrder = null,
            [AliasAs("type")] FulfillmentTaskType? type = null,
            [AliasAs("priority")] TaskPriority? priority = null,
            [AliasAs("expectedCompletionDate")] DateTimeOffset? expectedCompletionDate = null,
            [AliasAs("orderNumber")] string? orderNumber = null);

        [Post("/api/v1/fulfillment-tasks/procurement")]
        Task<Guid> CreateProcurementTask([Body] CreateProcurementTaskCommand task);

        [Post("/api/v1/fulfillment-tasks/production")]
        Task<Guid> CreateProductionTask([Body] CreateProductionTaskCommand task);

        [Get("/api/v1/users")]
        Task<List<UserDto>> GetUsers();

        [Get("/api/v1/products/search")]
        Task<PagedResult<ProductDto>> GetProductsPaged(
            int page,
            int pageSize,
            [AliasAs("searchTerm")] string? searchTerm = null,
            [AliasAs("sortColumn")] string? sortColumn = null,
            [AliasAs("sortOrder")] string? sortOrder = null,
            [AliasAs("stockFilterStatus")] StockFilterStatus stockFilterStatus = StockFilterStatus.All,
            [AliasAs("isActive")] bool? isActive = null,
            [AliasAs("categoryId")] Guid? categoryId = null,
            [AliasAs("hasVariants")] bool? hasVariants = null,
            [AliasAs("includeVariantsAndImages")] bool? includeVariantsAndImages = null);

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
    }
}
