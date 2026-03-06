using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public partial class ProductsViewModel : ObservableObject, INavigationAware
{
    private readonly IProductApi _productApi;
    private readonly IProductCategoryLookupApi _productCategoryLookupApi;
    private readonly IPermissionService _permissionService;
    private readonly IToastService _toastService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    // --- Data ---
    private ObservableCollection<ProductDto> _items = new();
    public ObservableCollection<ProductDto> Items 
    { 
        get => _items; 
        set => SetProperty(ref _items, value); 
    }

    // --- Pagination ---
    private int _page = 1;
    public int Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
            }
        }
    }

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    private bool _hasNextPage;
    public bool HasNextPage
    {
        get => _hasNextPage;
        set => SetProperty(ref _hasNextPage, value);
    }

    private bool _hasPreviousPage;
    public bool HasPreviousPage
    {
        get => _hasPreviousPage;
        set => SetProperty(ref _hasPreviousPage, value);
    }

    public ObservableCollection<int> PageSizes { get; } = new(new[] { 10, 25, 50, 100 });
    
    // --- Sorting ---
    private string? _sortColumn;
    private string? _sortOrder;

    // --- Filtering ---
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
    
    private ProductCategoryDto? _selectedCategory;
    public ProductCategoryDto? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
            }
        }
    }
    
    // Using string "Active", "Inactive", "All" for simple UI binding
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

    private string _selectedHasVariants = "All";
    public string SelectedHasVariants
    {
        get => _selectedHasVariants;
        set
        {
            if (SetProperty(ref _selectedHasVariants, value))
            {
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
            }
        }
    }
    public ObservableCollection<string> HasVariantsOptions { get; } = new(new[] { "Yes", "No", "All" });

    // --- UI State ---
    private bool _isListLoading;
    public bool IsListLoading
    {
        get => _isListLoading;
        set => SetProperty(ref _isListLoading, value);
    }
    public bool HasNoData => Items.Count == 0 && !IsListLoading;
    public string PageSummary => $"Page {Page} (Total: {TotalCount})";

    // --- Permissions ---
    public bool CanCreateProduct => _permissionService.CanExecute(Permissions.Products.Create);

    // --- Commands ---
    public IAsyncRelayCommand LoadDataCommand { get; }
    public IAsyncRelayCommand NextPageCommand { get; }
    public IAsyncRelayCommand PreviousPageCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand ClearFiltersCommand { get; }
    public IRelayCommand NewProductCommand { get; }

    public ProductsViewModel(
        IProductApi productApi,
        IProductCategoryLookupApi productCategoryLookupApi,
        IPermissionService permissionService,
        IToastService toastService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _productApi = productApi;
        _productCategoryLookupApi = productCategoryLookupApi;
        _permissionService = permissionService;
        _toastService = toastService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        NextPageCommand = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
        NewProductCommand = new RelayCommand(NavigateToNewProduct);
    }

    public async void OnNavigatedTo(object? parameter)
    {
        await LoadDataAsync();
    }

    public async void OnNavigatedFrom() { }



    public async Task LoadDataAsync()
    {
        if (IsListLoading) return;

        try
        {
            IsListLoading = true;
            OnPropertyChanged(nameof(HasNoData));

            bool? activeFilter = SelectedStatus switch
            {
                "Active" => true,
                "Inactive" => false,
                _ => null
            };

            bool? hasVariantsFilter = SelectedHasVariants switch
            {
                "Yes" => true,
                "No" => false,
                _ => null
            };

            var result = await _productApi.SearchProducts(
                page: Page,
                pageSize: PageSize,
                searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                sortColumn: _sortColumn,
                sortOrder: _sortOrder,
                isActive: activeFilter,
                categoryId: SelectedCategory?.Id,
                hasVariants: hasVariantsFilter);

            Items.Clear();
            
            var canView = _permissionService.CanExecute(Permissions.Products.View);
            var canEdit = _permissionService.CanExecute(Permissions.Products.Edit);
            
            foreach (var item in result.Items)
            {
                // Hydrate permission properties for data grid row actions
                item.CanEdit = canEdit;
                item.CanActivate = canEdit && !item.IsActive;
                item.CanDeactivate = canEdit && item.IsActive;
                
                Items.Add(item);
            }

            Page = result.Page;
            TotalCount = result.TotalCount;
            HasNextPage = result.HasNextPage;
            HasPreviousPage = result.HasPreviousPage;

            OnPropertyChanged(nameof(PageSummary));
        }
        catch (Exception ex)
        {
            // ProblemDetailsHandler catches the actual error.
            System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
        }
        finally
        {
            IsListLoading = false;
            OnPropertyChanged(nameof(HasNoData));
        }
    }

    public async Task SortByAsync(string column, string order)
    {
        _sortColumn = column;
        _sortOrder = order;
        Page = 1;
        await LoadDataAsync();
    }

    private async Task NextPageAsync()
    {
        if (HasNextPage)
        {
            Page++;
            await LoadDataAsync();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (HasPreviousPage)
        {
            Page--;
            await LoadDataAsync();
        }
    }

    private async Task RefreshAsync()
    {
        Page = 1;
        SearchTerm = string.Empty;
        SelectedCategory = null;
        SelectedStatus = "Active";
        SelectedHasVariants = "All";
        _sortColumn = null;
        _sortOrder = null;
        await LoadDataAsync();
    }

    private async Task ClearFiltersAsync()
    {
        SearchTerm = string.Empty;
        SelectedCategory = null;
        SelectedStatus = "All";
        SelectedHasVariants = "All";
        Page = 1;
        await LoadDataAsync();
    }

    private void NavigateToNewProduct()
    {
         _navigationService.NavigateTo(typeof(CreateProductViewModel).FullName!);
    }

    public async Task ActivateProductAsync(ProductDto product)
    {
        try
        {
            IsListLoading = true;
            await _productApi.ActivateProduct(product.Id);
            _toastService.ShowSuccess("Success", $"Product {product.Name} activated.");
            await LoadDataAsync();
        }
        catch (Exception)
        {
            // Handled by global handler
        }
        finally
        {
            IsListLoading = false;
        }
    }

    public async Task DeactivateProductAsync(ProductDto product)
    {
        try
        {
            var confirm = await _dialogService.ShowConfirmationAsync(
                "Deactivate Product",
                $"Are you sure you want to deactivate {product.Name}? This will hide it from the active catalog and deactivate all its variants.",
                "Deactivate", "Cancel");

            if (!confirm) return;

            IsListLoading = true;
            await _productApi.DeactivateProduct(product.Id);
            _toastService.ShowSuccess("Success", $"Product {product.Name} deactivated.");
            await LoadDataAsync();
        }
        catch (Exception)
        {
            // Handled by global handler
        }
        finally
        {
            IsListLoading = false;
        }
    }

    // --- Delegate for LeafProductCategoryPicker ---
    public async Task<PagedResult<ProductCategoryDto>> FetchCategoriesAsync(string searchTerm, int page, int pageSize, CancellationToken token = default)
    {
        try
        {
            return await _productCategoryLookupApi.SearchCategories(
                page: page,
                pageSize: pageSize,
                searchTerm: string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                isActive: true,
                isLeafOnly: true); // Crucial filter for the Leaf picker
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching categories: {ex.Message}");
            return new PagedResult<ProductCategoryDto> { Items = new(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }
    }
}
