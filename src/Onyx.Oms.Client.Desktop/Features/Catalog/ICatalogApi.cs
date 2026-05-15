using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Catalog;


public interface ICatalogApi
{
    [Get("/api/v1/catalog/summary")]
    Task<CatalogSummaryDto> GetCatalogSummary();

    [Get("/api/v1/catalog/dashboard/summary")]
    Task<CatalogDashboardSummaryDto> GetCatalogDashbaordSummary([Query] int lowStockThreshold);

    [Get("/api/v1/catalog/dashboard/alerts")]
    Task<CatalogDashboardAlertsDto> GetCatalogDashboardAlerts([Query] int lowStockThreshold, [Query] int limit);
}

public record CatalogSummaryDto(
        int TotalCategories,
        int TotalLeafCategories,
        int TotalProducts,
        int ActiveProducts,
        int TotalActiveVariants,
        int OutOfStockVariants,
        int LowStockVariants,
        int ProductsWithoutImages,
        int CategoriesWithoutProducts,
        int InactiveProducts,
        int TotalStockOnHand,
        int TotalReservedQuantity);

public record CatalogDashboardSummaryDto(
        int TotalVariantCount,
        int InactiveVariantCount,
        int OutOfStockCount,
        int LowStockCount,
        InboundSummaryDto Inbound,
        StockTotalsDto StockTotals,
        FulfillmentTasksSummaryDto FulfillmentTasks);
public record InboundSummaryDto(int VariantCount, int TotalUnits);
public record StockTotalsDto(int StockOnHand, int ReservedStock, int AvailableStock);
public record FulfillmentTasksSummaryDto(int InProduction, int Procurement);

public record CatalogDashboardAlertsDto(
        List<StockAlertItemDto> OutOfStock,
        List<StockAlertItemDto> LowStock);
public record StockAlertItemDto(
    Guid ProductId,
    string ProductName,
    Guid VariantId,
    string VariantLabel,
    int AvailableStock);
