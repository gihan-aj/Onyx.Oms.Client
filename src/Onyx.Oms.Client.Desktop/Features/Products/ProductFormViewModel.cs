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

public partial class ProductFormViewModel : ObservableObject, INavigationAware
{
    private readonly IProductApi _productApi;
    private readonly IProductCategoryLookupApi _productCategoryLookupApi;
    private readonly ITenantProfileService _tenantProfileService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastService;

    public ProductFormViewModel(
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
        
        SelectedColors.CollectionChanged += (s, e) => HasUnsavedChanges = true;
        SelectedSizes.CollectionChanged += (s, e) => HasUnsavedChanges = true;
        Variants.CollectionChanged += (s, e) => HasUnsavedChanges = true;
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
    public ProductCategoryDto? SelectedCategory { get => _selectedCategory; set { if (SetProperty(ref _selectedCategory, value)) HasUnsavedChanges = true; } }

    private string? _brand;
    public string? Brand { get => _brand; set { if (SetProperty(ref _brand, value)) HasUnsavedChanges = true; } }

    private string? _material;
    public string? Material { get => _material; set { if (SetProperty(ref _material, value)) HasUnsavedChanges = true; } }

    // Gender Enum: Unknown=0, Male=1, Female=2, Unisex=3
    private int _selectedGender = 0;
    public int SelectedGender { get => _selectedGender; set { if (SetProperty(ref _selectedGender, value)) HasUnsavedChanges = true; } }
    
    // --- Pricing & Shipping ---
    private decimal _baseCostAmount;
    public decimal BaseCostAmount { get => _baseCostAmount; set { if (SetProperty(ref _baseCostAmount, value)) HasUnsavedChanges = true; } }

    private decimal _basePriceAmount;
    public decimal BasePriceAmount { get => _basePriceAmount; set { if (SetProperty(ref _basePriceAmount, value)) HasUnsavedChanges = true; } }

    private decimal _baseWeightValue;
    public decimal BaseWeightValue { get => _baseWeightValue; set { if (SetProperty(ref _baseWeightValue, value)) HasUnsavedChanges = true; } }

    public string BaseCurrency => _tenantProfileService.Profile?.BaseCurrency ?? "LKR";
    public string BaseWeightUnit => _tenantProfileService.Profile?.WeightUnit ?? "kg";

    // --- Options & Variants ---
    private bool _hasColor;
    public bool HasColor { get => _hasColor; set { if (SetProperty(ref _hasColor, value)) HasUnsavedChanges = true; } }

    private bool _hasSize;
    public bool HasSize { get => _hasSize; set { if (SetProperty(ref _hasSize, value)) HasUnsavedChanges = true; } }

    // Using strictly observable collections for token inputs
    public ObservableCollection<string> SelectedColors { get; } = new();
    public ObservableCollection<string> SelectedSizes { get; } = new();
    
    public ObservableCollection<VariantDraftDto> Variants { get; } = new();

    // --- Images ---
    public ObservableCollection<CreateProductImageDto> Images { get; } = new();

    // --- Commands ---
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand CancelCommand { get; }
    public IRelayCommand GenerateMatrixCommand { get; }

    public void OnNavigatedTo(object parameter)
    {
        HasUnsavedChanges = false;
    }

    public void OnNavigatedFrom() { }

    private void GenerateMatrix()
    {
        Variants.Clear();
        HasUnsavedChanges = true;

        if (!HasColor && !HasSize)
        {
            Variants.Add(new VariantDraftDto 
            {
                CostAmount = BaseCostAmount,
                PriceAmount = BasePriceAmount,
                WeightValue = BaseWeightValue,
                StockOnHand = 0
            });
            return;
        }

        var colors = HasColor && SelectedColors.Any() ? SelectedColors.ToList() : new List<string> { null! };
        var sizes = HasSize && SelectedSizes.Any() ? SelectedSizes.ToList() : new List<string> { null! };

        foreach (var color in colors)
        {
            foreach (var size in sizes)
            {
                Variants.Add(new VariantDraftDto
                {
                    Color = color,
                    Size = size,
                    CostAmount = BaseCostAmount,
                    PriceAmount = BasePriceAmount,
                    WeightValue = BaseWeightValue,
                    StockOnHand = 0
                });
            }
        }
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Name) || SelectedCategory == null)
        {
            _toastService.ShowError("Validation Error", "Name and Category are required.");
            return;
        }

        try
        {
            IsBusy = true;

            var command = new CreateProductCommand(
                Name: Name,
                BaseSku: BaseSku,
                Description: Description,
                CategoryId: SelectedCategory.Id,
                Brand: Brand,
                Material: Material,
                Gender: SelectedGender,
                BaseCostAmount: BaseCostAmount,
                BaseCostCurrency: BaseCurrency,
                BasePriceAmount: BasePriceAmount,
                BasePriceCurrency: BaseCurrency,
                BaseWeightValue: BaseWeightValue,
                BaseWeightUnit: BaseWeightUnit,
                HasColor: HasColor,
                HasSize: HasSize,
                Tags: new List<string>(), // Feature for later
                Variants: Variants.Select(v => new CreateProductVariantDto(
                    v.Sku, v.Color, v.Size, v.CostAmount, v.PriceAmount, v.WeightValue, v.StockOnHand
                )).ToList(),
                Images: Images.ToList()
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

public partial class VariantDraftDto : ObservableObject
{
    private string? _sku;
    public string? Sku 
    {   get => _sku; 
        set 
        { 
            if (SetProperty(ref _sku, value)) OnPropertyChanged(nameof(IsDirty)); 
        } 
    }
    
    private string? _color;
    public string? Color { get => _color; set { if (SetProperty(ref _color, value)) OnPropertyChanged(nameof(IsDirty)); } }
    
    private string? _size;
    public string? Size { get => _size; set { if (SetProperty(ref _size, value)) OnPropertyChanged(nameof(IsDirty)); } }
    
    private decimal _costAmount;
    public decimal CostAmount { get => _costAmount; set { if (SetProperty(ref _costAmount, value)) OnPropertyChanged(nameof(IsDirty)); } }
    
    private decimal _priceAmount;
    public decimal PriceAmount { get => _priceAmount; set { if (SetProperty(ref _priceAmount, value)) OnPropertyChanged(nameof(IsDirty)); } }
    
    private decimal _weightValue;
    public decimal WeightValue { get => _weightValue; set { if (SetProperty(ref _weightValue, value)) OnPropertyChanged(nameof(IsDirty)); } }
    
    private int _stockOnHand;
    public int StockOnHand { get => _stockOnHand; set { if (SetProperty(ref _stockOnHand, value)) OnPropertyChanged(nameof(IsDirty)); } }

    public bool IsDirty { get; set; } // Used to trigger collection change parent event if needed, but standard bindings will work too.
}
