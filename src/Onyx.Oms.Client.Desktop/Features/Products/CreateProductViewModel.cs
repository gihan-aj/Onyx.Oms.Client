using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public partial class CreateProductViewModel : ObservableObject, INavigationAware
{
    private readonly IProductApi _productApi;
    private readonly IProductCategoryLookupApi _productCategoryLookupApi;
    private readonly ITenantProfileService _tenantProfileService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastService;

    public CreateProductViewModel(
        IProductApi productApi,
        IProductCategoryLookupApi productCategoryLookupApi,
        ITenantProfileService tenantProfileService,
        IDialogService dialogService,
        INavigationService navigationService,
        IToastService toastService)
    {
        _productApi = productApi;
        _productCategoryLookupApi = productCategoryLookupApi;
        _tenantProfileService = tenantProfileService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _toastService = toastService;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new AsyncRelayCommand(CancelAsync);
        GenerateMatrixCommand = new RelayCommand(GenerateMatrix);
        AddOptionCommand = new RelayCommand(AddOption);
        RemoveOptionCommand = new RelayCommand<ProductOptionModel>(RemoveOption);

        VariantDrafts.CollectionChanged += (s, e) => HasUnsavedChanges = true;
        ProductOptions.CollectionChanged += (s, e) => HasUnsavedChanges = true;
        Tags.CollectionChanged += (s, e) => HasUnsavedChanges = true;
    }

    private bool _hasUnsavedChanges;
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set => SetProperty(ref _hasUnsavedChanges, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    // --- Form Properties ---
    private string _name = string.Empty;
    public string Name { get => _name; set { if (SetProperty(ref _name, value)) HasUnsavedChanges = true; } }

    private string? _baseSku;
    public string? BaseSku { get => _baseSku; set { if (SetProperty(ref _baseSku, value)) HasUnsavedChanges = true; } }

    private string? _description;
    public string? Description { get => _description; set { if (SetProperty(ref _description, value)) HasUnsavedChanges = true; } }

    private ProductCategoryDto? _selectedCategory;
    public ProductCategoryDto? SelectedCategory 
    { 
        get => _selectedCategory; 
        set 
        { 
            if (SetProperty(ref _selectedCategory, value))
            {
                HasUnsavedChanges = true;
                if (value != null)
                {
                    _ = LoadCategorySpecificationsAsync(value.Id);
                }
                else
                {
                    DynamicSpecs.Clear();
                }
            }
        } 
    }
    
    public string BaseCurrency => _tenantProfileService.Profile?.BaseCurrency ?? "LKR";
    public string BaseWeightUnit => _tenantProfileService.Profile?.WeightUnit ?? "kg";

    // --- Pricing & Shipping ---
    private decimal _baseCostAmount;
    public decimal BaseCostAmount { get => _baseCostAmount; set { if (SetProperty(ref _baseCostAmount, value)) HasUnsavedChanges = true; } }

    private decimal _basePriceAmount;
    public decimal BasePriceAmount { get => _basePriceAmount; set { if (SetProperty(ref _basePriceAmount, value)) HasUnsavedChanges = true; } }

    private bool _isPhysicalProduct = true;
    public bool IsPhysicalProduct
    {
        get => _isPhysicalProduct;
        set
        {
            if (SetProperty(ref _isPhysicalProduct, value))
            {
                HasUnsavedChanges = true;
                if (!value) BaseWeightValue = 0; // Clear weight if not physical
            }
        }
    }

    private decimal _baseWeightValue;
    public decimal BaseWeightValue { get => _baseWeightValue; set { if (SetProperty(ref _baseWeightValue, value)) HasUnsavedChanges = true; } }

    private int _baseStockOnHand;
    public int BaseStockOnHand { get => _baseStockOnHand; set { if (SetProperty(ref _baseStockOnHand, value)) HasUnsavedChanges = true; } }

    // --- Options & Variants ---
    private bool _hasVariants;
    public bool HasVariants
    {
        get => _hasVariants;
        set
        {
            if (SetProperty(ref _hasVariants, value))
            {
                HasUnsavedChanges = true;
                if (!value)
                {
                    ProductOptions.Clear();
                    VariantDrafts.Clear();
                }
            }
        }
    }

    // Draft properties for creating a new option
    private string _draftOptionName = string.Empty;
    public string DraftOptionName { get => _draftOptionName; set => SetProperty(ref _draftOptionName, value); }
    
    public ObservableCollection<string> DraftOptionValues { get; } = new();

    public ObservableCollection<ProductOptionModel> ProductOptions { get; } = new();
    public ObservableCollection<VariantDraftModel> VariantDrafts { get; } = new();
    public ObservableCollection<CreateProductImageDto> Images { get; } = new();
    public ObservableCollection<string> Tags { get; } = new();
    public ObservableCollection<SpecFieldViewModel> DynamicSpecs { get; } = new();

    // --- Commands ---
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand CancelCommand { get; }
    public IRelayCommand GenerateMatrixCommand { get; }
    public IRelayCommand AddOptionCommand { get; }
    public IRelayCommand<ProductOptionModel> RemoveOptionCommand { get; }

    public void OnNavigatedTo(object parameter)
    {
        HasUnsavedChanges = false;
    }

    public void OnNavigatedFrom() { }

    private async Task LoadCategorySpecificationsAsync(Guid categoryId)
    {
        try
        {
            IsBusy = true;
            var response = await _productCategoryLookupApi.GetCategoryById(categoryId, includeParentSpecs: true);
            
            DynamicSpecs.Clear();
            foreach (var spec in response.AllSpecifications)
            {
                var vm = new SpecFieldViewModel
                {
                    Key = spec.Key,
                    Label = spec.Label,
                    Type = spec.Type,
                    IsRequired = spec.IsRequired,
                    Options = new ObservableCollection<string>(spec.Options)
                };
                DynamicSpecs.Add(vm);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading specifications: {ex.Message}");
            // Optional: User-friendly message here, but often handled implicitly or safely ignored if network error.
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AddOption()
    {
        if (string.IsNullOrWhiteSpace(DraftOptionName) || !DraftOptionValues.Any())
        {
            _toastService.ShowWarning("Incomplete Info", "Please provide both an Option Name and at least one Value.");
            return;
        }

        var valuesList = DraftOptionValues
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (!valuesList.Any())
        {
            _toastService.ShowWarning("Incomplete Info", "Please provide valid values.");
            return;
        }
        
        if (ProductOptions.Any(o => o.Name.Equals(DraftOptionName, StringComparison.OrdinalIgnoreCase)))
        {
            _toastService.ShowWarning("Duplicate Option", "An option with this name already exists.");
            return;
        }

        if(ProductOptions.Count() >= 3)
        {
            _toastService.ShowWarning("Maximum Options Reached", "You can only have up to 3 option axes for a product.");
            return;
        }

        ProductOptions.Add(new ProductOptionModel { Name = DraftOptionName, Values = valuesList });
        HasUnsavedChanges = true;
        
        DraftOptionName = string.Empty;
        DraftOptionValues.Clear();
    }

    private void RemoveOption(ProductOptionModel? option)
    {
        if (option != null)
        {
            ProductOptions.Remove(option);
            VariantDrafts.Clear();
            HasUnsavedChanges = true;
        }
    }

    private void GenerateMatrix()
    {
        if (!ProductOptions.Any())
        {
            _toastService.ShowWarning("No Options", "Please define at least one option axis to generate variants.");
            return;
        }

        VariantDrafts.Clear();
        HasUnsavedChanges = true;

        // Cartesian product generation
        var combinations = GetCombinations(ProductOptions.ToList());

        foreach (var combo in combinations)
        {
            // Build the display attributes string e.g. "Red / M"
            var display = string.Join(" / ", combo.Select(a => a.Value));
            
            // Build auto-suggested SKU snippet if BaseSku is provided
            string? overrideSku = null;
            if (!string.IsNullOrWhiteSpace(BaseSku))
            {
                var suffix = string.Join("-", combo.Select(a => a.Value.ToUpper().Replace(" ", "")));
                overrideSku = $"{BaseSku}-{suffix}";
            }

            VariantDrafts.Add(new VariantDraftModel
            {
                Attributes = combo,
                DisplayAttributes = display,
                Sku = overrideSku,
                CostAmount = BaseCostAmount,
                PriceAmount = BasePriceAmount,
                WeightValue = BaseWeightValue,
                StockOnHand = 0
            });
        }
    }

    // Helper to generate cartesian product of N lists
    private List<List<VariantAttributeDto>> GetCombinations(List<ProductOptionModel> options)
    {
        var result = new List<List<VariantAttributeDto>>();
        if (options.Count == 0) return result;

        void Permute(int depth, List<VariantAttributeDto> current)
        {
            if (depth == options.Count)
            {
                result.Add(new List<VariantAttributeDto>(current));
                return;
            }

            var currentOption = options[depth];
            foreach (var value in currentOption.Values)
            {
                current.Add(new VariantAttributeDto(currentOption.Name, value));
                Permute(depth + 1, current);
                current.RemoveAt(current.Count - 1);
            }
        }

        Permute(0, new List<VariantAttributeDto>());
        return result;
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Name) || SelectedCategory == null)
        {
            _toastService.ShowError("Validation Error", "Product Name and Category are required.");
            return;
        }

        if (HasVariants && !VariantDrafts.Any())
        {
             _toastService.ShowError("Validation Error", "You must generate the variant matrix before saving a product with variations.");
             return;
        }

        try
        {
            IsBusy = true;

            int? stockOnHand = HasVariants ? null : BaseStockOnHand;
            
            var optionsDto = HasVariants && ProductOptions.Any() 
                ? ProductOptions.Select(o => new ProductOptionDto(o.Name, o.Values)).ToList() 
                : null;

            var variantsDto = HasVariants && VariantDrafts.Any()
                ? VariantDrafts.Select(v => new CreateProductVariantDto(
                    Sku: v.Sku,
                    Attributes: v.Attributes,
                    Cost: new MoneyDto(v.CostAmount,BaseCurrency),
                    Price: new MoneyDto(v.PriceAmount, BaseCurrency),
                    Weight: new WeightDto(v.WeightValue, BaseWeightUnit),
                    StockOnHand: v.StockOnHand
                )).ToList()
                : null;

            var specsDictionary = new Dictionary<string, string>();
            foreach (var field in DynamicSpecs)
            {
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    specsDictionary[field.Key] = field.Value;
                }
                else if (field.IsRequired)
                {
                    _toastService.ShowError("Validation Error", $"The specification '{field.Label}' is required.");
                    IsBusy = false;
                    return;
                }
            }

            var command = new CreateProductCommand(
                Name: Name,
                BaseSku: BaseSku,
                Description: Description,
                CategoryId: SelectedCategory.Id,
                BaseCost: new MoneyDto(BaseCostAmount, BaseCurrency),
                BasePrice: new MoneyDto(BasePriceAmount, BaseCurrency),
                BaseWeight: IsPhysicalProduct ? new WeightDto(BaseWeightValue, BaseWeightUnit) : null, // Ensure weight is 0 if digital
                HasVariants: HasVariants,
                BaseStockOnHand: stockOnHand,
                Options: optionsDto,
                Specifications: specsDictionary,
                Variants: variantsDto,
                Images: Images.ToList(),
                Tags: Tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
            );

            var productId = await _productApi.CreateProduct(command);
            
            HasUnsavedChanges = false;
            _toastService.ShowSuccess("Success", "Product created successfully.");
            _navigationService.GoBack();
        }
        catch (Exception ex)
        {
            // Global ProblemDetailsHandler handles the UI error.
            System.Diagnostics.Debug.WriteLine($"Save Failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        if (HasUnsavedChanges)
        {
            var confirm = await _dialogService.ShowConfirmationAsync(
                "Unsaved Changes",
                "You have unsaved changes. Are you sure you want to discard them and leave?",
                "Discard",
                "Stay");

            if (!confirm) return;
        }

        HasUnsavedChanges = false;
        _navigationService.GoBack();
    }

    // Delegate for Category Picker
    public async Task<PagedResult<ProductCategoryDto>> FetchCategoriesAsync(string searchTerm, int page, int pageSize)
    {
        try
        {
            return await _productCategoryLookupApi.SearchCategories(page, pageSize, searchTerm, null, null, true, null, true);
        }
        catch
        {
            return new PagedResult<ProductCategoryDto>() { Items = new(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }
    }
}

// UI Wrapper Models for better data binding
public partial class ProductOptionModel : ObservableObject
{
    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    
    private List<string> _values = new();
    public List<string> Values { get => _values; set => SetProperty(ref _values, value); }
}

public partial class VariantDraftModel : ObservableObject
{
    public List<VariantAttributeDto> Attributes { get; init; } = new();
    public string DisplayAttributes { get; init; } = string.Empty;
    
    private string? _sku;
    public string? Sku { get => _sku; set => SetProperty(ref _sku, value); }
    
    private decimal _costAmount;
    public decimal CostAmount { get => _costAmount; set => SetProperty(ref _costAmount, value); }
    
    private decimal _priceAmount;
    public decimal PriceAmount { get => _priceAmount; set => SetProperty(ref _priceAmount, value); }
    
    private decimal _weightValue;
    public decimal WeightValue { get => _weightValue; set => SetProperty(ref _weightValue, value); }
    
    private int _stockOnHand;
    public int StockOnHand { get => _stockOnHand; set => SetProperty(ref _stockOnHand, value); }
}
