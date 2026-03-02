using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.ProductCategories;

public partial class ProductCategoriesViewModel : ObservableObject, INavigationAware
{
    private readonly IProductCategoryApi _api;
    private readonly IPermissionService _permissionService;
    private readonly IToastService _toastService;
    private readonly IDialogService _dialogService;

    public ProductCategoriesViewModel(
        IProductCategoryApi api,
        IPermissionService permissionService,
        IToastService toastService,
        IDialogService dialogService)
    {
        _api = api;
        _permissionService = permissionService;
        _toastService = toastService;
        _dialogService = dialogService;

        RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
        ActivateCommand = new AsyncRelayCommand<ProductCategoryTreeDto>(ActivateCategory);
        DeactivateCommand = new AsyncRelayCommand<ProductCategoryTreeDto>(DeactivateCategory);
        DeleteCommand = new AsyncRelayCommand<ProductCategoryTreeDto>(DeleteCategory);
    }

    private ProductCategoryTreeDto? _selectedCategory;
    public ProductCategoryTreeDto? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                OnPropertyChanged(nameof(HasSelection));
                _ = LoadCategoryDetailsAsync();
            }
        }
    }

    private ProductCategoryResponse? _selectedCategoryDetails;
    public ProductCategoryResponse? SelectedCategoryDetails
    {
        get => _selectedCategoryDetails;
        private set
        {
            if (SetProperty(ref _selectedCategoryDetails, value))
            {
                OnPropertyChanged(nameof(HasCategoryDetails));
            }
        }
    }

    public bool HasCategoryDetails => _selectedCategoryDetails != null;

    private async Task LoadCategoryDetailsAsync()
    {
        if (_selectedCategory == null)
        {
            SelectedCategoryDetails = null;
            return;
        }

        try
        {
            IsLoading = true;
            SelectedCategoryDetails = await _api.GetCategoryById(_selectedCategory.Id);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load category details: {ex.Message}");
            SelectedCategoryDetails = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public bool HasSelection => SelectedCategory != null;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            SetProperty(ref _isLoading, value);
            OnPropertyChanged(nameof(HasNoCategories));
        }
    }

    public bool HasNoCategories => Categories.Count == 0 && !IsLoading;

    // Use SetProperty pattern for ObservableCollection as recommended by the guide for WinRT
    private ObservableCollection<ProductCategoryTreeDto> _categories = new();
    public ObservableCollection<ProductCategoryTreeDto> Categories
    {
        get => _categories;
        set
        {
            SetProperty(ref _categories, value);
            OnPropertyChanged(nameof(HasNoCategories));
        }
    }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand<ProductCategoryTreeDto> ActivateCommand { get; }
    public IAsyncRelayCommand<ProductCategoryTreeDto> DeactivateCommand { get; }
    public IAsyncRelayCommand<ProductCategoryTreeDto> DeleteCommand { get; }

    public void OnNavigatedTo(object parameter)
    {
        RefreshCommand.Execute(null);
    }

    public void OnNavigatedFrom()
    {
        // Cleanup if necessary
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            SelectedCategory = null; // Clear selection on refresh
            
            var tree = await _api.GetCategoryTree();
            
            var canEdit = _permissionService.CanExecute(Permissions.ProductCategories.Edit);
            var canDelete = _permissionService.CanExecute(Permissions.ProductCategories.Delete);
            var canActivate = _permissionService.CanExecute(Permissions.ProductCategories.Activate);
            var canDeactivate = _permissionService.CanExecute(Permissions.ProductCategories.Deactivate);
            var canToggle = canActivate && canDeactivate;
            var canCreate = _permissionService.CanExecute(Permissions.ProductCategories.Create);

            // Calculate permissions
            PopulateTreeMetadata(tree, canEdit, canDelete, canToggle, canCreate);

            Categories = new ObservableCollection<ProductCategoryTreeDto>(tree);
        }
        catch (Exception ex)
        {
            // Error handling is handled globally via ProblemDetailsHandler, but log locally if needed
            System.Diagnostics.Debug.WriteLine($"Failed to load categories: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void PopulateTreeMetadata(
        System.Collections.Generic.IEnumerable<ProductCategoryTreeDto> nodes, 
        bool canEdit, bool canDelete, bool canToggle, bool canCreate)
    {
        foreach (var node in nodes)
        {
            node.CanEdit = canEdit;
            node.CanDelete = canDelete;
            node.CanToggleStatus = canToggle;

            if (node.SubCategories != null && node.SubCategories.Count > 0)
            {
                PopulateTreeMetadata(node.SubCategories, canEdit, canDelete, canToggle, canCreate);
            }
        }
    }

    public async Task ActivateCategory(ProductCategoryTreeDto? category)
    {
        if (category == null || IsLoading) return;

        try
        {
            IsLoading = true;
            await _api.ActivateCategory(category.Id);
            _toastService.ShowSuccess("Success", "Category activated successfully.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error activating category: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DeactivateCategory(ProductCategoryTreeDto? category)
    {
        if (category == null || IsLoading) return;

        var confirm = await _dialogService.ShowConfirmationAsync(
            "Deactivate Category",
            $"Are you sure you want to deactivate the category '{category.Name}'?"
        );

        if (!confirm) return;

        try
        {
            IsLoading = true;
            await _api.DeactivateCategory(category.Id);
            _toastService.ShowSuccess("Success", "Category deactivated successfully.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deactivating category: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DeleteCategory(ProductCategoryTreeDto? category)
    {
        if (category == null || IsLoading) return;

        var confirm = await _dialogService.ShowConfirmationAsync(
            "Delete Category",
            $"Are you sure you want to delete the category '{category.Name}'? This action cannot be undone."
        );

        if (!confirm) return;

        try
        {
            IsLoading = true;
            await _api.DeleteCategory(category.Id);
            _toastService.ShowSuccess("Success", "Category deleted successfully.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting category: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
