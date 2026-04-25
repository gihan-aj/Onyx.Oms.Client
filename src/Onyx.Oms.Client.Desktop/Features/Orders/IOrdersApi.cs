using Onyx.Oms.Client.Desktop.Features.Products;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders
{
    public interface IOrdersApi
    {
        [Get("/api/v1/orders")]
        Task<PagedResult<OrderSummaryDto>> GetOrdersPaged(
            int page,
            int pageSize,
            [AliasAs("searchTerm")] string? searchTerm = null,
            [AliasAs("sortColumn")] string? sortColumn = null,
            [AliasAs("sortOrder")] string? sortOrder = null,
            [AliasAs("status")] OrderStatus? status = null,
            [AliasAs("paymentStatus")] PaymentStatus? paymentStatus = null,
            [AliasAs("customerId")] Guid? customerId = null,
            [AliasAs("fromDate")] DateTimeOffset? fromDate = null,
            [AliasAs("toDate")] DateTimeOffset? toDate = null,
            [AliasAs("includeDetails")] bool includeDetails = false);

        [Post("/api/v1/orders")]
        Task<Guid> CreateOrder([Body] CreateOrderCommand task);

        [Get("/api/v1/customers/search")]
        Task<PagedResult<CustomerDto>> SearchCustomers(
            [AliasAs("Page")] int page,
            [AliasAs("PageSize")] int pageSize,
            [AliasAs("SearchTerm")] string? searchTerm = null,
            [AliasAs("SortColumn")] string? sortColumn = null,
            [AliasAs("SortOrder")] string? sortOrder = null,
            [AliasAs("IsActive")] bool? isActive = null);

        [Post("/api/v1/customers")]
        Task<Guid> CreateCustomer([Body] CreateCustomerCommand customer);

        [Get("/api/v1/customers/{id}")]
        Task<CustomerDto> GetCustomerById(Guid id);

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

        [Get("/api/v1/couriers")]
        Task<List<CourierDto>> GetCouriers([AliasAs("IsActive")] bool isActive = true);
    }
}
