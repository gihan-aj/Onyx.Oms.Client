using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Features.Products.Create;
using Onyx.Oms.Client.Desktop.Features.Products.Details;
using Onyx.Oms.Client.Desktop.Features.Products.Edit;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using static Onyx.Oms.Client.Desktop.Shared.Constants.Permissions;

namespace Onyx.Oms.Client.Desktop.Features.Products.List;

public partial class ProductsViewModel : PagedDataGridViewModelBase<ProductGridItem>, INavigationAware
{
    private readonly IProductsApi _productsApi;
    private readonly IProductCategoryLookupApi _categoryLookupApi;
    private readonly IPermissionService _permissionService;
    private readonly IFileService _fileService;
    private readonly IDialogService _dialogService;
    private readonly IToastService _toastService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<ProductsViewModel> _logger;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    // -- Filtering --
    private string _searchTerm = string.Empty;
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
            }
        }
    }

    private ProductCategoryDto? _selectedProductCategory;
    public ProductCategoryDto? SelectedProductCategory
    {
        get => _selectedProductCategory;
        set
        {
            if (SetProperty(ref _selectedProductCategory, value))
            {
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
            }
        }
    }

    private string _selectedStatus = "Active";
    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
            {
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
            }
        }
    }

    public ObservableCollection<string> StatusOptions { get; } = new(new[] { "Active", "Inactive", "All" });

    public ObservableCollection<StockStatusOption> StockStatusOptions { get; } = new(new[]
    {
        new StockStatusOption("All Stock", StockFilterStatus.All),
        new StockStatusOption("In Stock", StockFilterStatus.InStock),
        new StockStatusOption("Low Stock", StockFilterStatus.LowStock),
        new StockStatusOption("Out of Stock", StockFilterStatus.OutOfStock)
    });

    private StockStatusOption _selectedStockStatus;
    public StockStatusOption SelectedStockStatus
    {
        get => _selectedStockStatus;
        set
        {
            if (SetProperty(ref _selectedStockStatus, value))
            {
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
            }
        }
    }

    // --- Permissions ---
    public bool CanCreateProducts => _permissionService.CanExecute(Permissions.Products.Create);
    public bool CanEditProducts => _permissionService.CanExecute(Permissions.Products.Edit);
    public bool CanActivateProducts => _permissionService.CanExecute(Permissions.Products.Activate);
    public bool CanDeactivateProducts => _permissionService.CanExecute(Permissions.Products.Deactivate);

    // -- Commands --
    public IAsyncRelayCommand ClearFiltersCommand { get; }
    public IRelayCommand NewProductCommand { get; }
    public IRelayCommand<ProductGridItem> ViewDetailsCommand { get; }
    public IRelayCommand<ProductGridItem> DownloadAndOpenProductSheetCommand { get; }
    public IRelayCommand<ProductGridItem> EditDetailsCommand { get; }

    public ProductsViewModel(
        IProductsApi productsApi,
        IPermissionService permissionService,
        IProductCategoryLookupApi categoryLookupApi,
        IFileService fileService,
        IDialogService dialogService,
        IToastService toastService,
        INavigationService navigationService,
        ILogger<ProductsViewModel> logger)
    {
        _productsApi = productsApi;
        _permissionService = permissionService;
        _categoryLookupApi = categoryLookupApi;
        _fileService = fileService;
        _dialogService = dialogService;
        _toastService = toastService;
        _navigationService = navigationService;
        _logger = logger;

        _selectedStockStatus = StockStatusOptions[0];

        ClearFiltersCommand = new AsyncRelayCommand(ClearFlitersAsync);
        NewProductCommand = new RelayCommand(NavigateToNewProduct);
        ViewDetailsCommand = new RelayCommand<ProductGridItem>(ViewProductDetails);
        EditDetailsCommand = new RelayCommand<ProductGridItem>(EditProductDetails);
        DownloadAndOpenProductSheetCommand = new AsyncRelayCommand<ProductGridItem>(DownloadAndOpenSheetAsync);
    }



    public void OnNavigatedFrom()
    {

    }

    public async void OnNavigatedTo(object parameter)
    {
        bool triggeredReload = false;
        if (parameter is StockFilterStatus statusFilter)
        {
            var option = StockStatusOptions.FirstOrDefault(x => x.Value == statusFilter);
            if (option != null && _selectedStockStatus != option)
            {
                SelectedStockStatus = option;
                triggeredReload = true;
            }
        }

        if (!triggeredReload)
        {
            await LoadDataAsync();
        }
    }

    protected override async Task LoadDataAsync()
    {
        if (IsListLoading)
            return;

        try
        {
            IsListLoading = true;

            bool? activeFilter = SelectedStatus switch
            {
                "Active" => true,
                "Inactive" => false,
                _ => null,
            };

            var result = await _productsApi.GetProductsPaged(
                page: Page,
                pageSize: PageSize,
                searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                sortColumn: SortColumn,
                sortOrder: SortOrder,
                stockFilterStatus: SelectedStockStatus?.Value ?? StockFilterStatus.All,
                isActive: activeFilter,
                categoryId: SelectedProductCategory?.Id);

            Items.Clear();

            foreach (var item in result.Items)
            {
                var gridItem = await item.ToGridItem(CanEditProducts, CanActivateProducts, CanDeactivateProducts, _fileService);
                Items.Add(gridItem);
            }

            Page = result.Page;
            TotalCount = result.TotalCount;
            HasNextPage = result.HasNextPage;
            HasPreviousPage = result.HasPreviousPage;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
        }
        finally
        {
            IsListLoading = false;
        }
    }

    protected override Task OnRefreshFiltersAsync()
    {
        SearchTerm = string.Empty;
        SelectedProductCategory = null;
        SelectedStatus = "Active";
        SelectedStockStatus = StockStatusOptions[0];
        return Task.CompletedTask;
    }

    private async Task ClearFlitersAsync()
    {
        SearchTerm = string.Empty;
        SelectedProductCategory = null;
        SelectedStatus = "All";
        SelectedStockStatus = StockStatusOptions[0];
        Page = 1;
        await LoadDataAsync();
    }

    public async Task<PagedResult<ProductCategoryDto>> FetchCategoriesAsync(string searchTerm, int page, int pageSize, bool isLeafOnly, CancellationToken token = default)
    {
        try
        {
            return await _categoryLookupApi.SearchCategories(
                page: page,
                pageSize: pageSize,
                searchTerm: string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                isActive: true,
                isLeafOnly: isLeafOnly);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching categories: {ex.Message}");
            return new PagedResult<ProductCategoryDto> { Items = new(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }
    }

    private void NavigateToNewProduct()
    {
        _navigationService.NavigateTo(typeof(CreateProductViewModel).FullName!);
    }

    private void ViewProductDetails(ProductGridItem? product)
    {
        if (product != null)
        {
            _navigationService.NavigateTo(typeof(ProductDetailsViewModel).FullName!, product.Id);
        }
    }

    private void EditProductDetails(ProductGridItem? product)
    {
        if (product != null)
        {
            _navigationService.NavigateTo(typeof(EditProductViewModel).FullName!, product.Id);
        }
    }

    private async Task DownloadAndOpenSheetAsync(ProductGridItem? product)
    {
        if (product == null) return;

        try
        {
            IsBusy = true;

            var imageFilePath = ApplicationData.Current.LocalFolder.Path + "\\ProductImages";
            var response = await _productsApi.GetProductSheetById(product.Id, imageFilePath);

            if (response.IsSuccessStatusCode)
            {
                // Read the raw PDF stream into a byte array
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    // Save and launch the file just like before
                    string safeFileName = $"ProductSheet_{product.BaseSku ?? product.Id.ToString().Substring(0, 8)}.pdf";
                    string tempFilePath = Path.Combine(Path.GetTempPath(), safeFileName);

                    await File.WriteAllBytesAsync(tempFilePath, pdfBytes);

                    var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(tempFilePath);
                    await Windows.System.Launcher.LaunchFileAsync(storageFile);
                }
            }
            else
            {
                _logger.LogWarning("API returned {StatusCode} when downloading product sheet.", response.StatusCode);
                _toastService.ShowError("Download Failed", "Failed to download product sheet. Please check logs for information.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download product sheet for {ProductId}", product.Id);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ActivateProductAsync(ProductGridItem? product)
    {
        if (product != null)
        {
            try
            {
                IsListLoading = true;
                await _productsApi.ActivateProduct(product.Id);
                IsListLoading = false;
                _toastService.ShowSuccess("Success", $"Product {product.Name} activated.");
                await LoadDataAsync();
            }
            catch (Exception)
            {
            }
            finally
            {
                IsListLoading = false;
            }
        }
    }

    public async Task DeactivateProductAsync(ProductGridItem? product)
    {
        if (product != null)
        {
            try
            {
                var confirm = await _dialogService.ShowConfirmationAsync(
                    "Deactivate Product",
                    $"Are you sure you want to deactivate {product.Name}? This will hide it from the active catalog and deactivate all its variants.",
                    "Deactivate", "Cancel");

                if (!confirm) return;

                IsListLoading = true;

                await _productsApi.DeactivateProduct(product.Id);
                IsListLoading = false;
                _toastService.ShowSuccess("Success", $"Product {product.Name} deactivated.");
                await LoadDataAsync();
            }
            catch (Exception)
            {
            }
            finally
            {
                IsListLoading = false;
            }
        }
    }
}

public record StockStatusOption(string DisplayName, StockFilterStatus Value);