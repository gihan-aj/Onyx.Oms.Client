using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Features.Products.Create;
using Onyx.Oms.Client.Desktop.Features.Products.Details;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
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

    // -- Filtering --
    private string _searchTerm = string.Empty;
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if(SetProperty(ref _searchTerm, value))
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
            if(SetProperty(ref _selectedProductCategory, value))
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
            if(SetProperty(ref _selectedStatus, value))
            {
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
            }
        }
    }

    public ObservableCollection<string> StatusOptions { get; } = new(new[] { "Active", "Inactive", "All" });

    // --- Permissions ---
    public bool CanCreateProducts => _permissionService.CanExecute(Permissions.Products.Create);
    public bool CanEditProducts => _permissionService.CanExecute(Permissions.Products.Edit);
    public bool CanActivateProducts => _permissionService.CanExecute(Permissions.Products.Activate);
    public bool CanDeactivateProducts => _permissionService.CanExecute(Permissions.Products.Deactivate);

    // -- Commands --
    public IAsyncRelayCommand ClearFiltersCommand { get; }
    public IRelayCommand NewProductCommand { get; }
    public IRelayCommand<ProductGridItem> ViewDetailsCommand {  get; }
    public IRelayCommand<ProductGridItem> EditDetailsCommand {  get; }

    public ProductsViewModel(
        IProductsApi productsApi,
        IPermissionService permissionService,
        IProductCategoryLookupApi categoryLookupApi,
        IFileService fileService,
        IDialogService dialogService,
        IToastService toastService,
        INavigationService navigationService)
    {
        _productsApi = productsApi;
        _permissionService = permissionService;
        _categoryLookupApi = categoryLookupApi;
        _fileService = fileService;
        _dialogService = dialogService;
        _toastService = toastService;
        _navigationService = navigationService;

        ClearFiltersCommand = new AsyncRelayCommand(ClearFlitersAsync);
        NewProductCommand = new RelayCommand(NavigateToNewProduct);
        ViewDetailsCommand = new RelayCommand<ProductGridItem>(ViewProductDetails);
        EditDetailsCommand = new RelayCommand<ProductGridItem>(EditProductDetails);
    }



    public void OnNavigatedFrom()
    {
        
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadDataAsync();
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
                isActive: activeFilter,
                categoryId: SelectedProductCategory?.Id);

            Items.Clear();

            foreach(var item in result.Items)
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
        return Task.CompletedTask;
    }

    private async Task ClearFlitersAsync()
    {
        SearchTerm = string.Empty;
        SelectedProductCategory = null;
        SelectedStatus = "All";
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
        if(product != null)
        {
            _navigationService.NavigateTo(typeof(ProductDetailsViewModel).FullName!, product.Id);
        }
    }

    private void EditProductDetails(ProductGridItem? product)
    {
        if(product != null)
        {

        }
    }

    public async Task ActivateProductAsync(ProductGridItem? product)
    {
        if( product != null)
        {
            try
            {
                IsListLoading = true;
                await _productsApi.ActivateProduct(product.Id);
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
        if(product != null)
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