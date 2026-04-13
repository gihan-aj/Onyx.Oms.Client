using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System;
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
    }
}
