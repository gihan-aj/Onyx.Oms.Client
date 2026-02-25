using Refit;
using System;
using System.Threading.Tasks;
using Onyx.Oms.Client.Desktop.Shared.Models;

namespace Onyx.Oms.Client.Desktop.Features.Customers;

public interface ICustomerApi
{
    [Get("/api/v1/customers/search")]
    Task<PagedResult<CustomerDto>> SearchCustomers(
        [AliasAs("Page")] int page,
        [AliasAs("PageSize")] int pageSize,
        [AliasAs("SearchTerm")] string? searchTerm = null,
        [AliasAs("SortColumn")] string? sortColumn = null,
        [AliasAs("SortOrder")] string? sortOrder = null,
        [AliasAs("IsActive")] bool? isActive = null);

    [Get("/api/v1/customers/{id}")]
    Task<CustomerDto> GetCustomerById(Guid id);

    [Post("/api/v1/customers")]
    Task<Guid> CreateCustomer([Body] CreateCustomerDto customer);

    [Put("/api/v1/customers/{id}")]
    Task UpdateCustomer(Guid id, [Body] UpdateCustomerDto customer);

    [Put("/api/v1/customers/{id}/activate")]
    Task ActivateCustomer(Guid id);

    [Put("/api/v1/customers/{id}/deactivate")]
    Task DeactivateCustomer(Guid id);

    [Delete("/api/v1/customers/{id}")]
    Task DeleteCustomer(Guid id);
}
