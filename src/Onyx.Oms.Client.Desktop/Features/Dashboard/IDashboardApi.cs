using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Dashboard;

public interface IDashboardApi
{
    // Placeholder calls for dashboard data

    [Get("/api/v1/dashboard/recent-orders")]
    Task<List<DashboardItem>> GetRecentOrdersAsync();

    [Get("/api/v1/dashboard/pending-tasks")]
    Task<List<DashboardItem>> GetPendingTasksAsync();
}
