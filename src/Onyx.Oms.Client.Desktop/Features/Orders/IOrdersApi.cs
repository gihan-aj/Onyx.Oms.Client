using Onyx.Oms.Client.Desktop.Features.Products;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
            [Query(CollectionFormat.Multi)] OrderStatus[]? statuses = null,
            [AliasAs("paymentStatus")] PaymentStatus? paymentStatus = null,
            [AliasAs("customerId")] Guid? customerId = null,
            [AliasAs("courierId")] Guid? courierId = null,
            [AliasAs("isCashOnDelivery")] bool? isCashOnDelivery = null,
            [AliasAs("fromDate")] DateTimeOffset? fromDate = null,
            [AliasAs("toDate")] DateTimeOffset? toDate = null,
            [AliasAs("includeDetails")] bool includeDetails = false);

        [Get("/api/v1/orders/status-counts")]
        Task<GetOrderStatusCountsResponse> GetOrderStatusCount(
            [AliasAs("searchTerm")] string? searchTerm = null,
            [AliasAs("paymentStatus")] PaymentStatus? paymentStatus = null,
            [AliasAs("customerId")] Guid? customerId = null,
            [AliasAs("courierId")] Guid? courierId = null,
            [AliasAs("isCashOnDelivery")] bool? isCashOnDelivery = null,
            [AliasAs("fromDate")] DateTimeOffset? fromDate = null,
            [AliasAs("toDate")] DateTimeOffset? toDate = null);

        [Get("/api/v1/orders/{id}")]
        Task<OrderDetailsDto> GetOrderById(Guid id);

        [Get("/api/v1/orders/{id}/invoice")]
        Task<HttpResponseMessage> GetOrderInvoiceById(Guid id, [Query] string logoStoragePath);

        [Get("/api/v1/orders/{id}/shipping-label")]
        Task<HttpResponseMessage> GetShippingLabelById(Guid id);

        [Post("/api/v1/orders")]
        Task<Guid> CreateOrder([Body] CreateOrderCommand task);

        [Put("/api/v1/orders/{id}/financials")]
        Task UpdateFinancials(Guid id, [Body] UpdateOrderFinancialsCommand command);

        [Put("/api/v1/orders/{id}/logistics")]
        Task UpdateLogistics(Guid id, [Body] UpdateOrderLogisticsCommand command);

        [Post("/api/v1/orders/{id}/payments")]
        Task<Guid> AddPayment(Guid id, [Body] AddPaymentCommand command);

        [Put("/api/v1/orders/{id}/notes")]
        Task UpdateNotes(Guid id, [Body] UpdateOrderNotesCommand command);

        [Put("/api/v1/orders/{orderId}/items/{orderItemId}/allocate-quantity")]
        Task AllocateOrderItemQuantity(Guid orderId, Guid orderItemId, [Body] AllocateOrderItemQuantityCommand command);

        [Post("/api/v1/orders/{orderId}/items/{orderItemId}/production-tasks")]
        Task CreateProductionTask(Guid orderId, Guid orderItemId, [Body] CreateOrderProductionTaskCommand command);

        [Post("/api/v1/orders/{orderId}/items/{orderItemId}/procurement-tasks")]
        Task CreateProcurementTask(Guid orderId, Guid orderItemId, [Body] CreateOrderProcurementTaskCommand command);

        [Post("/api/v1/orders/{id}/confirm")]
        Task ConfirmOrder(Guid id);

        [Post("/api/v1/orders/{id}/cancel")]
        Task CancelOrder(Guid id, [Body] CancelOrderCommand command);

        [Post("/api/v1/orders/{id}/complete")]
        Task CompleteOrder(Guid id);

        [Post("/api/v1/orders/{id}/deliver")]
        Task DeliverOrder(Guid id);

        [Post("/api/v1/orders/{id}/fail-delivery")]
        Task FailOrderDelivery(Guid id, [Body] FailDeliveryCommand command);

        [Post("/api/v1/orders/{id}/pack")]
        Task PackOrder(Guid id);

        [Post("/api/v1/orders/{id}/ship")]
        Task ShipOrder(Guid id, [Body] ShipOrderCommand command);

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

        [Get("/api/v1/customers/{id}/orders")]
        Task<CustomerOrderHistoryResponse> GetCustomerOrderHistory(Guid id, int top = 20);

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

        [Get("/api/v1/couriers/{id}")]
        Task<CourierDto> GetCourier(Guid id);
    }
}
