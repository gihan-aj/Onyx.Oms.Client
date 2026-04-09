using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Features.ProductCategories;
using Onyx.Oms.Client.Desktop.Features.Products;
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

    private CatalogSummaryDto? _summary;
    public CatalogSummaryDto? Summary
    {
        get => _summary;
        set
        {
            if(SetProperty(ref _summary, value))
            {
                OnPropertyChanged(nameof(ActiveProductsText));
                OnPropertyChanged(nameof(HasLowStockAlert));
                OnPropertyChanged(nameof(LowStockText));
            }
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ActiveProductsText => Summary != null ? $"{Summary.ActiveProducts} currently active" : "";
    public string LowStockText => Summary != null ? $"{Summary.LowStockVariants} variants running low" : "";
    public bool HasLowStockAlert => Summary != null && Summary.LowStockVariants > 0;

    public ObservableCollection<CatalogNavigationItem> NavigationItems { get; } = new();

    public IRelayCommand NavigateToOutOfStockCommand { get; }
    public IRelayCommand NavigateToLowStockCommand { get; }
    public IRelayCommand NavigateToProductsCommand { get; }
    public IRelayCommand NavigateToCategoriesCommand { get; }
    public IRelayCommand NavigateToInStockCommand { get; }

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

        NavigateToOutOfStockCommand = new RelayCommand(NavigateToOutOfStock);
        NavigateToLowStockCommand = new RelayCommand(NavigateToLowStock);
        NavigateToProductsCommand = new RelayCommand(NavigateToProducts);
        NavigateToCategoriesCommand = new RelayCommand(NavigateToCategories);
        NavigateToInStockCommand = new RelayCommand(NavigateToInStock);

        // Setup the navigation cards
        NavigationItems.Add(new CatalogNavigationItem(
            Title: "Products",
            Description: "View inventory, update pricing, and manage individual product details.",
            IconGlyph: "\uE719",
            Command: new RelayCommand(NavigateToProducts)
        ));

        NavigationItems.Add(new CatalogNavigationItem(
            Title: "Categories",
            Description: "Manage product groupings, attributes, and hierarchical structures.",
            IconGlyph: "\xE81E",
            Command: new RelayCommand(NavigateToCategories)
        ));
    }

    public async void OnNavigatedTo(object? parameter)
    {
        await LoadSummaryDataAsync();
    }

    private async Task LoadSummaryDataAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _catalogApi.GetCatalogSummary();

            Summary = result;

            //OnPropertyChanged(nameof(ActiveProductsText));
            //OnPropertyChanged(nameof(LowStockText));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void NavigateToProducts()
    {
        _navigationService.NavigateTo(typeof(ProductsPage).FullName!);
    }

    private void NavigateToCategories()
    {
        _navigationService.NavigateTo(typeof(ProductCategoriesPage).FullName!);
    }

    private void NavigateToOutOfStock()
    {
        _navigationService.NavigateTo(typeof(ProductsPage).FullName!, StockFilterStatus.OutOfStock);
    }

    private void NavigateToLowStock()
    {
        _navigationService.NavigateTo(typeof(ProductsPage).FullName!, StockFilterStatus.LowStock);
    }

    private void NavigateToInStock()
    {
        _navigationService.NavigateTo(typeof(ProductsPage).FullName!, StockFilterStatus.InStock);
    }

    public void OnNavigatedFrom()
    {

    }
}
