using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Catalog;

public partial class CatalogViewModel : ObservableObject
{
    private readonly IPermissionService _permissionService;
    private readonly ICatalogApi _catalogApi;
    private readonly ILogger<CatalogViewModel> _logger;

    public ObservableCollection<CatalogCardItem> CatalogItems { get; } = new();

    public CatalogViewModel(
        IPermissionService permissionService,
        ICatalogApi catalogApi,
        ILogger<CatalogViewModel> logger)
    {
        _permissionService = permissionService;
        _catalogApi = catalogApi;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        CatalogItems.Clear();

        // Check Permissions before adding cards
        if (_permissionService.HasFeatureAccess("tenant:productcategories:"))
        {
            CatalogItems.Add(new CatalogCardItem
            {
                Title = "Categories",
                Description = "Organize products into hierarchical groups.",
                IconGlyph = "\uF168", // Folder
                TargetPageType = typeof(ProductCategories.ProductCategoriesPage).FullName!,
                MetricValue = "..."
            });
        }

        if (_permissionService.HasFeatureAccess("tenant:products:"))
        {
            CatalogItems.Add(new CatalogCardItem
            {
                Title = "Products",
                Description = "Manage your core product inventory and details.",
                IconGlyph = "\uE719", // Box Open
                TargetPageType = "Onyx.Oms.Client.Desktop.Features.Products.List.ProductsPage",
                MetricValue = "..."
            });
        }

        // Add variants here if they have a separate permission, or tie it to Products

        // Fetch metrics in the background
        await FetchMetricsAsync();
    }

    private async Task FetchMetricsAsync()
    {
        try
        {
            var summary = await _catalogApi.GetCatalogSummary();

            foreach (var item in CatalogItems)
            {
                if (item.Title == "Categories")
                {
                    item.MetricValue = $"{summary.TotalCategories} active";
                }
                else if (item.Title == "Products")
                {
                    item.MetricValue = $"{summary.TotalProducts} active";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch catalog metrics. The API might not be implemented yet.");
            
            // Fallback to empty string so the UI doesn't show "..." forever
            foreach (var item in CatalogItems)
            {
                item.MetricValue = string.Empty;
            }
        }
    }
}
