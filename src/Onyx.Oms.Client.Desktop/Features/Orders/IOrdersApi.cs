using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
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
        Task<Guid> CreateProcurementTask([Body] CreateOrderCommand task);

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
    }
}
