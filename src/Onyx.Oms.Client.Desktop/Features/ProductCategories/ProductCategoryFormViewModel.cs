using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.ProductCategories;

public class SpecDefinitionViewModel : ObservableObject
{
    private string _key = string.Empty;
    public string Key
    {
        get => _key;
        set => SetProperty(ref _key, value);
    }

    private string _label = string.Empty;
    public string Label
    {
        get => _label;
        set => SetProperty(ref _label, value);
    }

    private SpecType _type = SpecType.Text;
    public SpecType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    private bool _isRequired;
    public bool IsRequired
    {
        get => _isRequired;
        set => SetProperty(ref _isRequired, value);
    }

    private string _optionsString = string.Empty;
    public string OptionsString
    {
        get => _optionsString;
        set => SetProperty(ref _optionsString, value);
    }

    public SpecDefinition ToModel()
    {
        return new SpecDefinition
        {
            Key = Key,
            Label = Label,
            Type = Type,
            IsRequired = IsRequired,
            Options = string.IsNullOrWhiteSpace(OptionsString) 
                ? new List<string>() 
                : OptionsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
        };
    }

    public static SpecDefinitionViewModel FromModel(SpecDefinition model)
    {
        return new SpecDefinitionViewModel
        {
            Key = model.Key,
            Label = model.Label,
            Type = model.Type,
            IsRequired = model.IsRequired,
            OptionsString = string.Join(", ", model.Options ?? new List<string>())
        };
    }
}

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

    private bool _hasProducts;
    public bool HasProducts
    {
        get => _hasProducts;
        set
        {
            if (SetProperty(ref _hasProducts, value))
            {
                OnPropertyChanged(nameof(CanEditSpecifications));
            }
        }
    }

    public bool CanEditSpecifications => !HasProducts;

    public ObservableCollection<SpecDefinitionViewModel> Specifications { get; } = new();

    public IReadOnlyList<SpecType> AvailableSpecTypes { get; } = Enum.GetValues<SpecType>();

    public Func<string, int, int, CancellationToken, Task<PagedResult<ProductCategoryDto>>> FetchParentCategories => async (searchTerm, page, pageSize, token) =>
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
    public IRelayCommand AddSpecificationCommand { get; }
    public IRelayCommand<SpecDefinitionViewModel> RemoveSpecificationCommand { get; }

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
        AddSpecificationCommand = new RelayCommand(OnAddSpecificationExecute);
        RemoveSpecificationCommand = new RelayCommand<SpecDefinitionViewModel>(OnRemoveSpecificationExecute);
    }

    private void OnAddSpecificationExecute()
    {
        var spec = new SpecDefinitionViewModel();
        Specifications.Add(spec);
    }

    private void OnRemoveSpecificationExecute(SpecDefinitionViewModel? spec)
    {
        if (spec != null)
        {
            Specifications.Remove(spec);
        }
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
                
                // Fetch full details including specifications and HasProducts
                var fullCategory = await _api.GetCategoryById(CategoryId.Value);
                
                if (fullCategory != null)
                {
                    Name = fullCategory.Name;
                    Description = fullCategory.Description;
                    DisplayOrder = fullCategory.DisplayOrder;
                    IconUrl = fullCategory.IconUrl;
                    Color = fullCategory.Color;
                    SelectedParentCategory = allCategories.FirstOrDefault(p => p.Id == fullCategory.ParentCategoryId);
                    Title = $"Edit Category ({Name})";
                    HasProducts = fullCategory.HasProducts;

                    Specifications.Clear();
                    foreach (var spec in fullCategory.Specifications ?? new List<SpecDefinition>())
                    {
                        Specifications.Add(SpecDefinitionViewModel.FromModel(spec));
                    }
                }
            }
            else
            {
                IsEditMode = false;
                Title = "Create Category";
                DisplayOrder = 0; // Default
                HasProducts = false;
                Specifications.Clear();
                
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
            // Check for duplicate specification keys
            var duplicateKeys = Specifications
                .Where(s => !string.IsNullOrWhiteSpace(s.Key))
                .GroupBy(s => s.Key)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateKeys.Any())
            {
                _toastService.ShowError("Validation Error", $"Duplicate specification keys found: {string.Join(", ", duplicateKeys)}");
                return false;
            }

            var specs = Specifications.Select(s => s.ToModel()).ToList();

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
                    ParentCategoryId = SelectedParentCategory?.Id,
                    Specifications = specs
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
                    ParentCategoryId = SelectedParentCategory?.Id,
                    Specifications = specs
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
