using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.ProductCategories;

public class ProductCategoryFormNavArgs
{
    public Guid? CategoryId { get; set; } // Null if Create
    public Guid? PreselectedParentId { get; set; } // Only used on Create
}

public partial class ProductCategoryFormViewModel : ObservableObject, INavigationAware
{
    private readonly IProductCategoryApi _api;
    private readonly IToastService _toastService;
    private readonly ILogger<ProductCategoryFormViewModel> _logger;
    private readonly INavigationService _navigationService;

    public bool IsEditMode { get; private set; }
    public Guid? CategoryId { get; private set; }

    private string _title = "Create Category";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string? _description;
    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private int _displayOrder;
    public int DisplayOrder
    {
        get => _displayOrder;
        set => SetProperty(ref _displayOrder, value);
    }

    private string? _iconUrl;
    public string? IconUrl
    {
        get => _iconUrl;
        set => SetProperty(ref _iconUrl, value);
    }

    private string? _color;
    public string? Color
    {
        get => _color;
        set => SetProperty(ref _color, value);
    }

    private ProductCategoryDto? _selectedParentCategory;
    public ProductCategoryDto? SelectedParentCategory
    {
        get => _selectedParentCategory;
        set => SetProperty(ref _selectedParentCategory, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string? _nameError;
    public string? NameError
    {
        get => _nameError;
        set => SetProperty(ref _nameError, value);
    }

    public Func<string, int, int, Task<PagedResult<ProductCategoryDto>>> FetchParentCategories => async (searchTerm, page, pageSize) =>
    {
        var result = await _api.SearchCategories(page, pageSize, searchTerm: searchTerm, isValidParent: true, sortColumn: "namePath", sortOrder: "asc");
        
        if (IsEditMode && CategoryId.HasValue)
        {
            var validItems = result.Items.Where(c => c.Id != CategoryId.Value).ToList();
            return new PagedResult<ProductCategoryDto>
            {
                Items = validItems,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage
            };
        }

        return result;
    };

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public ProductCategoryFormViewModel(
        IProductCategoryApi api,
        IToastService toastService,
        ILogger<ProductCategoryFormViewModel> logger,
        INavigationService navigationService)
    {
        _api = api;
        _toastService = toastService;
        _logger = logger;
        _navigationService = navigationService;

        SaveCommand = new AsyncRelayCommand(OnSaveExecuteAsync);
        CancelCommand = new RelayCommand(OnCancelExecute);
    }

    public async Task InitializeAsync(ProductCategoryFormNavArgs args)
    {
        IsLoading = true;
        try
        {
            // Load flat list of categories to populate the initial edit state / pre-selections
            var allCategories = await _api.GetCategories();
            
            if (args.CategoryId.HasValue)
            {
                IsEditMode = true;
                CategoryId = args.CategoryId;
                
                // Find existing details from the flat list to populate form
                var existing = allCategories.FirstOrDefault(c => c.Id == CategoryId);
                if (existing != null)
                {
                    Name = existing.Name;
                    Description = existing.Description;
                    DisplayOrder = existing.DisplayOrder;
                    IconUrl = existing.IconUrl;
                    Color = existing.Color;
                    SelectedParentCategory = allCategories.FirstOrDefault(p => p.Id == existing.ParentCategoryId);
                    Title = $"Edit Category ({Name})";
                }
            }
            else
            {
                IsEditMode = false;
                Title = "Create Category";
                DisplayOrder = 0; // Default
                
                if (args.PreselectedParentId.HasValue)
                {
                    SelectedParentCategory = allCategories.FirstOrDefault(p => p.Id == args.PreselectedParentId.Value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize category form");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnCancelExecute()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    private async Task OnSaveExecuteAsync()
    {
        var result = await SaveAsync();
        if (result && _navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    public async Task<bool> SaveAsync()
    {
        IsLoading = true;
        NameError = null;

        try
        {
            if (IsEditMode)
            {
                var updateDto = new UpdateProductCategoryDto
                {
                    Id = CategoryId!.Value,
                    Name = Name,
                    Description = Description,
                    DisplayOrder = DisplayOrder,
                    IconUrl = string.IsNullOrWhiteSpace(IconUrl) ? null : IconUrl,
                    Color = string.IsNullOrWhiteSpace(Color) ? null : Color,
                    ParentCategoryId = SelectedParentCategory?.Id
                };
                await _api.UpdateCategory(updateDto.Id, updateDto);
                _toastService.ShowSuccess("Success", "Category updated successfully.");
            }
            else
            {
                var createDto = new CreateProductCategoryDto
                {
                    Name = Name,
                    Description = Description,
                    DisplayOrder = DisplayOrder,
                    IconUrl = string.IsNullOrWhiteSpace(IconUrl) ? null : IconUrl,
                    Color = string.IsNullOrWhiteSpace(Color) ? null : Color,
                    ParentCategoryId = SelectedParentCategory?.Id
                };
                await _api.CreateCategory(createDto);
                _toastService.ShowSuccess("Success", "Category created successfully.");
            }
            return true;
        }
        catch (Refit.ApiException ex)
        {
            var problemDetails = await ex.GetContentAsAsync<Shared.Models.ProblemDetails>();
            var errors = problemDetails?.Errors ?? problemDetails?.Extensions?.Errors;

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    if (string.Equals(error.Code, "Name", StringComparison.OrdinalIgnoreCase) ||
                        error.Description?.Contains("Name", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        NameError = error.Description;
                    }
                }
            }
            // Handled globally for toast
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save category");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is ProductCategoryFormNavArgs args)
        {
            await InitializeAsync(args);
        }
        else
        {
            await InitializeAsync(new ProductCategoryFormNavArgs());
        }
    }

    public void OnNavigatedFrom()
    {
    }
}
