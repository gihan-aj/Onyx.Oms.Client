using System;
using System.Threading.Tasks;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public interface ICourierApi
{
    [Get("/api/v1/couriers/search")]
    Task<PagedResult<CourierDto>> SearchCouriers(
        [AliasAs("Page")] int page, 
        [AliasAs("PageSize")] int pageSize, 
        [AliasAs("SearchTerm")] string? searchTerm = null, 
        [AliasAs("SortColumn")] string? sortColumn = null, 
        [AliasAs("SortOrder")] string? sortOrder = null);

    [Get("/api/v1/couriers/{id}")]
    Task<CourierDto> GetCourier(Guid id);

    [Delete("/api/v1/couriers/{id}")]
    Task DeleteCourier(Guid id);
}
