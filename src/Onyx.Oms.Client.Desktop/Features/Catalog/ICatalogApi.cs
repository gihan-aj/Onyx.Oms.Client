using Refit;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Catalog;

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

public interface ICatalogApi
{
    [Get("/api/v1/catalog/summary")]
    Task<CatalogSummaryDto> GetCatalogSummary();
}
