using Refit;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Catalog;

public class CatalogSummaryDto
{
    public int TotalCategories { get; set; }
    public int TotalProducts { get; set; }
    public int TotalVariants { get; set; }
}

public interface ICatalogApi
{
    [Get("/api/v1/catalog/summary")]
    Task<CatalogSummaryDto> GetCatalogSummary();
}
