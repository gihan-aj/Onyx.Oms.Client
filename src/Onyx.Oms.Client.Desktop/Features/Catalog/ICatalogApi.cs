using Refit;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Catalog;

public record CatalogSummaryDto(
        int TotalCategories,
        int TotalProducts,
        int ActiveProducts,
        int TotalActiveVariants,
        int OutOfStockVariants,
        int LowStockVariants);

public interface ICatalogApi
{
    [Get("/api/v1/catalog/summary")]
    Task<CatalogSummaryDto> GetCatalogSummary();
}
