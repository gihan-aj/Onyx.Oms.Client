using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List;
using Onyx.Oms.Client.Desktop.Features.ProductCategories;
using Onyx.Oms.Client.Desktop.Features.Products;
using Onyx.Oms.Client.Desktop.Features.Products.Edit;
using Onyx.Oms.Client.Desktop.Features.Products.List;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Catalog;

public partial class CatalogViewModel : ObservableObject, INavigationAware
{
    private readonly IPermissionService _permissionService;
    private readonly ICatalogApi _catalogApi;
    private readonly ILogger<CatalogViewModel> _logger;
    private readonly INavigationService _navigationService;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _lastSyncedText = "Not synced yet";
    public string LastSyncedText
    {
        get => _lastSyncedText;
        set => SetProperty(ref _lastSyncedText, value);
    }

    private CatalogDashboardSummaryDto? _dashboardSummary;
    public CatalogDashboardSummaryDto? DashboardSummary
    {
        get => _dashboardSummary;
        set => SetProperty(ref _dashboardSummary, value);
    }

    private CatalogDashboardAlertsDto? _alerts;
    public CatalogDashboardAlertsDto? Alerts
    {
        get => _alerts;
        set => SetProperty(ref _alerts, value);
    }

    private int _lowStockThreshold = 10;
    public int LowStockThreshold
    {
        get => _lowStockThreshold;
        set => SetProperty(ref _lowStockThreshold, value);
    }

    public bool HasOutOfStockAlerts => Alerts?.OutOfStock?.Count > 0;
    public bool HasLowStockAlerts => Alerts?.LowStock?.Count > 0;

    public IRelayCommand NavigateToCatalogCommand { get; }
    public IRelayCommand NavigateToStockoutsCommand { get; }
    public IRelayCommand NavigateToLowStockAlertsCommand { get; }
    public IRelayCommand NavigateToFulfillmentTasksCommand { get; }
    public IRelayCommand NavigateToStockReportCommand { get; }
    public IRelayCommand<StockAlertItemDto> NavigateToProductDetailCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    public CatalogViewModel(
        IPermissionService permissionService,
        ICatalogApi catalogApi,
        ILogger<CatalogViewModel> logger,
        INavigationService navigationService)
    {
        _permissionService = permissionService;
        _catalogApi = catalogApi;
        _logger = logger;
        _navigationService = navigationService;

        NavigateToCatalogCommand = new RelayCommand(NavigateToCatalog);
        NavigateToStockoutsCommand = new RelayCommand(NavigateToStockouts);
        NavigateToLowStockAlertsCommand = new RelayCommand(NavigateToLowStockAlerts);
        NavigateToFulfillmentTasksCommand = new RelayCommand(NavigateToFulfillmentTasks);
        NavigateToStockReportCommand = new RelayCommand(NavigateToStockReport);
        NavigateToProductDetailCommand = new RelayCommand<StockAlertItemDto>(NavigateToProductDetail);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    public async void OnNavigatedTo(object? parameter)
    {
        await LoadSummaryDataAsync();
    }

    private async Task LoadSummaryDataAsync()
    {
        try
        {
            IsLoading = true;

            var summaryTask = _catalogApi.GetCatalogDashbaordSummary(LowStockThreshold);
            var alertsTask = _catalogApi.GetCatalogDashboardAlerts(LowStockThreshold, 10);

            await Task.WhenAll(summaryTask, alertsTask);
            DashboardSummary = summaryTask.Result;
            Alerts = alertsTask.Result;

            OnPropertyChanged(nameof(HasOutOfStockAlerts));
            OnPropertyChanged(nameof(HasLowStockAlerts));

            LastSyncedText = $"Last synced at {DateTime.Now:h:mm tt}";
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to load catalog dashboard data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadSummaryDataAsync();
    }

    private void NavigateToCatalog()
    {
        _navigationService.NavigateTo(typeof(ProductsPage).FullName!);
    }

    private void NavigateToStockouts()
    {
        _navigationService.NavigateTo(typeof(ProductsPage).FullName!, StockFilterStatus.OutOfStock);
    }

    private void NavigateToLowStockAlerts()
    {
        _navigationService.NavigateTo(typeof(ProductsPage).FullName!, StockFilterStatus.LowStock);
    }

    private void NavigateToFulfillmentTasks()
    {
         _navigationService.NavigateTo(typeof(FulfillmentTasksPage).FullName!);
    }

    private void NavigateToStockReport()
    {
        _navigationService.NavigateTo(typeof(ProductsPage).FullName!, StockFilterStatus.InStock);
    }

    private void NavigateToProductDetail(StockAlertItemDto? alertItem)
    {
        if (alertItem != null)
        {
             _navigationService.NavigateTo(typeof(EditProductViewModel).FullName!, alertItem.ProductId);
        }
    }

    public void OnNavigatedFrom()
    {

    }
}
