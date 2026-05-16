using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Dashboard;

public interface IDashboardApi
{
    // Placeholder calls for dashboard data

    [Get("/api/v1/dashboard/summary")]
    Task<MainDashboardSummaryDto> GetDashboardSummaryAsync();

    [Get("/api/v1/dashboard/action-required")]
    Task<ActionRequiredListDto> GetActionRequiredAsync([Query]int limit = 5);

    [Get("/api/v1/dashboard/in-motion")]
    Task<InMotionListDto> GetInMotionAsync([Query]int limit = 5);
}

public record MainDashboardSummaryDto(
        MainDashboardUserDto User,
        int ActionRequiredCount,
        MainDashboardStatsDto Stats);
public record MainDashboardUserDto(string DisplayName);
public record MainDashboardStatsDto(
    int PendingOrders,
    int ReadyToPack,
    int TasksCompletedUnallocated,
    int ShippedToday);

public record ActionRequiredListDto(
        int Total,
        List<ActionRequiredItemDto> Items);
public record ActionRequiredItemDto(
    string Type,
    Guid OrderId,
    string? OrderNumber,
    string CustomerName,
    decimal TotalAmount,
    string Currency,
    string Status,
    string Reason,
    string ReasonLabel,
    DateTimeOffset? CreatedAt);

public record InMotionListDto(
        int Total,
        List<InMotionItemDto> Items);

public record InMotionItemDto(
    string Type,
    Guid? TaskId,
    string? VariantLabel,
    string? TaskType,
    string? TaskStatus,
    int? Quantity,
    string? LinkedOrderNumber,
    Guid? LinkedOrderId,
    bool? IsOrphaned,
    Guid? OrderId,
    string? OrderNumber,
    string? CustomerName,
    string? OrderStatus,
    string? TrackingNumber,
    string ContextLabel);
