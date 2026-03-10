using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public partial class EditProductViewModel : ObservableObject, INavigationAware
{
    private readonly IGetProductDetailsApi _getProductApi;
    private readonly IUpdateProductApi _updateProductApi;
    private readonly IProductCategoryLookupApi _productCategoryLookupApi;
    private readonly ITenantProfileService _tenantProfileService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastService;
    private readonly IFileService _fileService;
    private readonly ILogger<EditProductViewModel> _logger;

    public EditProductViewModel(
        IGetProductDetailsApi getProductApi,
        IUpdateProductApi updateProductApi,
        IProductCategoryLookupApi productCategoryLookupApi,
        ITenantProfileService tenantProfileService,
        IDialogService dialogService,
        INavigationService navigationService,
        IToastService toastService,
        IFileService fileService,
        ILogger<EditProductViewModel> logger)
    {
        _getProductApi = getProductApi;
        _updateProductApi = updateProductApi;
        _productCategoryLookupApi = productCategoryLookupApi;
        _tenantProfileService = tenantProfileService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _toastService = toastService;
        _fileService = fileService;
        _logger = logger;

        GoBackCommand = new RelayCommand(GoBack);
        
        EditBasicInfoCommand = new RelayCommand(BeginEditBasicInfo);
        CancelBasicInfoCommand = new RelayCommand(CancelEditBasicInfo);
        SaveBasicInfoCommand = new AsyncRelayCommand(SaveBasicInfoAsync);

        EditTagsCommand = new RelayCommand(BeginEditTags);
        CancelTagsCommand = new RelayCommand(CancelEditTags);
        SaveTagsCommand = new AsyncRelayCommand(SaveTagsAsync);

        EditSpecificationsCommand = new AsyncRelayCommand(BeginEditSpecificationsAsync);
        CancelSpecificationsCommand = new RelayCommand(CancelEditSpecifications);
        SaveSpecificationsCommand = new AsyncRelayCommand(SaveSpecificationsAsync);

        EditBaseLogisticsCommand = new RelayCommand(BeginEditBaseLogistics);
        CancelBaseLogisticsCommand = new RelayCommand(CancelEditBaseLogistics);
        SaveBaseLogisticsCommand = new AsyncRelayCommand(SaveBaseLogisticsAsync);

        ToggleHasVariantsCommand = new AsyncRelayCommand<bool>(ToggleHasVariantsAsync);

        EditDefaultVariantLogisticsCommand = new RelayCommand(BeginEditDefaultVariantLogistics);
        CancelDefaultVariantLogisticsCommand = new RelayCommand(CancelEditDefaultVariantLogistics);
        SaveDefaultVariantLogisticsCommand = new AsyncRelayCommand(SaveDefaultVariantLogisticsAsync);

        EditOptionsCommand = new RelayCommand(BeginEditOptions);
        CancelOptionsCommand = new RelayCommand(CancelEditOptions);
        SaveOptionsCommand = new AsyncRelayCommand(SaveOptionsAsync);
        AddOptionCommand = new RelayCommand(AddOption);
        RemoveOptionCommand = new RelayCommand<ProductOptionModel>(RemoveOption);

        EditVariantsCommand = new RelayCommand(BeginEditVariants);
        CancelVariantsCommand = new RelayCommand(CancelEditVariants);
        SaveVariantsCommand = new AsyncRelayCommand(SaveVariantsAsync);
    }

    private ProductDetailsDto? _product;
    public ProductDetailsDto? Product
    {
        get => _product;
        set
        {
            if (SetProperty(ref _product, value))
            {

            }
        }
    }

    private string _pageTitle = "Edit Product";
    public string PageTitle
    {
        get => _pageTitle;
        set => SetProperty(ref _pageTitle, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    // --- Basic Information Draft State ---
    
    private bool _isEditingBasicInfo;
    public bool IsEditingBasicInfo
    {
        get => _isEditingBasicInfo;
        set => SetProperty(ref _isEditingBasicInfo, value);
    }

    private string _draftName = string.Empty;
    public string DraftName { get => _draftName; set => SetProperty(ref _draftName, value); }

    private string? _draftDescription;
    public string? DraftDescription { get => _draftDescription; set => SetProperty(ref _draftDescription, value); }

    private string? _draftBaseSku;
    public string? DraftBaseSku { get => _draftBaseSku; set => SetProperty(ref _draftBaseSku, value); }

    private ProductCategoryDto? _draftCategory;
    public ProductCategoryDto? DraftCategory { get => _draftCategory; set => SetProperty(ref _draftCategory, value); }
    
    public IRelayCommand EditBasicInfoCommand { get; }
    public IRelayCommand CancelBasicInfoCommand { get; }
    public IAsyncRelayCommand SaveBasicInfoCommand { get; }

    // --- Specifications Draft State ---
    private bool _isEditingSpecifications;
    public bool IsEditingSpecifications
    {
        get => _isEditingSpecifications;
        set => SetProperty(ref _isEditingSpecifications, value);
    }

    public System.Collections.ObjectModel.ObservableCollection<SpecFieldViewModel> DraftSpecs { get; } = new();

    public IAsyncRelayCommand EditSpecificationsCommand { get; }
    public IRelayCommand CancelSpecificationsCommand { get; }
    public IAsyncRelayCommand SaveSpecificationsCommand { get; }

    // --- Tags Draft State ---
    private bool _isEditingTags;
    public bool IsEditingTags
    {
        get => _isEditingTags;
        set => SetProperty(ref _isEditingTags, value);
    }

    public System.Collections.ObjectModel.ObservableCollection<string> DraftTags { get; } = new();

    public IRelayCommand EditTagsCommand { get; }
    public IRelayCommand CancelTagsCommand { get; }
    public IAsyncRelayCommand SaveTagsCommand { get; }

    // --- Options Draft State ---
    private bool _isEditingOptions;
    public bool IsEditingOptions
    {
        get => _isEditingOptions;
        set => SetProperty(ref _isEditingOptions, value);
    }

    private string _draftOptionName = string.Empty;
    public string DraftOptionName { get => _draftOptionName; set => SetProperty(ref _draftOptionName, value); }

    public System.Collections.ObjectModel.ObservableCollection<string> DraftOptionValues { get; } = new();

    public System.Collections.ObjectModel.ObservableCollection<ProductOptionModel> DraftOptions { get; } = new();

    public IRelayCommand EditOptionsCommand { get; }
    public IRelayCommand CancelOptionsCommand { get; }
    public IAsyncRelayCommand SaveOptionsCommand { get; }
    public IRelayCommand AddOptionCommand { get; }
    public IRelayCommand<ProductOptionModel> RemoveOptionCommand { get; }

    // --- Variants Draft State ---
    private bool _isEditingVariants;
    public bool IsEditingVariants
    {
        get => _isEditingVariants;
        set => SetProperty(ref _isEditingVariants, value);
    }

    public System.Collections.ObjectModel.ObservableCollection<EditVariantDraftModel> DraftVariants { get; } = new();

    public IRelayCommand EditVariantsCommand { get; }
    public IRelayCommand CancelVariantsCommand { get; }
    public IAsyncRelayCommand SaveVariantsCommand { get; }

    // --- Base Logistics Draft State ---
    private bool _isEditingBaseLogistics;
    public bool IsEditingBaseLogistics
    {
        get => _isEditingBaseLogistics;
        set => SetProperty(ref _isEditingBaseLogistics, value);
    }

    private decimal _draftBaseCostAmount;
    public decimal DraftBaseCostAmount { get => _draftBaseCostAmount; set => SetProperty(ref _draftBaseCostAmount, value); }

    private decimal _draftBasePriceAmount;
    public decimal DraftBasePriceAmount { get => _draftBasePriceAmount; set => SetProperty(ref _draftBasePriceAmount, value); }

    private bool _draftIsPhysicalProduct;
    public bool DraftIsPhysicalProduct { get => _draftIsPhysicalProduct; set => SetProperty(ref _draftIsPhysicalProduct, value); }

    private decimal? _draftBaseWeightValue;
    public decimal? DraftBaseWeightValue { get => _draftBaseWeightValue; set => SetProperty(ref _draftBaseWeightValue, value); }

    public IRelayCommand EditBaseLogisticsCommand { get; }
    public IRelayCommand CancelBaseLogisticsCommand { get; }
    public IAsyncRelayCommand SaveBaseLogisticsCommand { get; }

    public IAsyncRelayCommand<bool> ToggleHasVariantsCommand { get; }

    // --- Default Variant Logistics Draft State ---
    private bool _isEditingDefaultVariantLogistics;
    public bool IsEditingDefaultVariantLogistics
    {
        get => _isEditingDefaultVariantLogistics;
        set => SetProperty(ref _isEditingDefaultVariantLogistics, value);
    }

    private string? _draftDefaultVariantSku;
    public string? DraftDefaultVariantSku { get => _draftDefaultVariantSku; set => SetProperty(ref _draftDefaultVariantSku, value); }

    private decimal _draftDefaultVariantCostAmount;
    public decimal DraftDefaultVariantCostAmount { get => _draftDefaultVariantCostAmount; set => SetProperty(ref _draftDefaultVariantCostAmount, value); }

    private decimal _draftDefaultVariantPriceAmount;
    public decimal DraftDefaultVariantPriceAmount { get => _draftDefaultVariantPriceAmount; set => SetProperty(ref _draftDefaultVariantPriceAmount, value); }

    private decimal? _draftDefaultVariantWeightValue;
    public decimal? DraftDefaultVariantWeightValue { get => _draftDefaultVariantWeightValue; set => SetProperty(ref _draftDefaultVariantWeightValue, value); }

    private int _draftDefaultVariantStockOnHand;
    public int DraftDefaultVariantStockOnHand { get => _draftDefaultVariantStockOnHand; set => SetProperty(ref _draftDefaultVariantStockOnHand, value); }

    public IRelayCommand EditDefaultVariantLogisticsCommand { get; }
    public IRelayCommand CancelDefaultVariantLogisticsCommand { get; }
    public IAsyncRelayCommand SaveDefaultVariantLogisticsCommand { get; }

    public IRelayCommand GoBackCommand { get; }

    private void GoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
        else
        {
            _navigationService.NavigateTo("Onyx.Oms.Client.Desktop.Features.Products.ProductsPage");
        }
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is Guid productId)
        {
            await LoadProductAsync(productId);
        }
        else if (parameter is string productIdString && Guid.TryParse(productIdString, out var parsedId))
        {
            await LoadProductAsync(parsedId);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task LoadProductAsync(Guid id)
    {
        try
        {
            IsLoading = true;
            Product = await _getProductApi.GetProductById(id);
            PageTitle = $"Edit {Product?.Name ?? "Unknown Product"}";

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load product details for {ProductId}", id);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BeginEditBasicInfo()
    {
        if (Product == null) return;
        
        DraftName = Product.Name;
        DraftDescription = Product.Description;
        DraftBaseSku = Product.BaseSku;
        
        // Construct a shallow category object for the picker
        DraftCategory = new ProductCategoryDto 
        { 
            Id = Product.CategoryId, 
            Name = Product.CategoryName, 
            Path = Product.CategoryPath 
        };
        
        IsEditingBasicInfo = true;
    }

    private void CancelEditBasicInfo()
    {
        IsEditingBasicInfo = false;
    }

    private async Task SaveBasicInfoAsync()
    {
        if (Product == null || IsLoading) return;

        if (string.IsNullOrWhiteSpace(DraftName))
        {
            _toastService.ShowError("Validation Error", "Product Name is required.");
            return;
        }

        if (DraftCategory == null)
        {
            _toastService.ShowError("Validation Error", "Product Category is required.");
            return;
        }

        try
        {
            IsLoading = true;

            var command = new UpdateProductBasicInfoCommand(
                Name: DraftName,
                Description: DraftDescription,
                BaseSku: DraftBaseSku,
                CategoryId: DraftCategory.Id,
                Tags: Product.Tags
            );

            await _updateProductApi.UpdateBasicInformation(Product.Id, command);

            // Optimistically update the local read model
            Product = Product with 
            { 
                Name = DraftName, 
                Description = DraftDescription, 
                BaseSku = DraftBaseSku ?? string.Empty,
                CategoryId = DraftCategory.Id,
                CategoryName = DraftCategory.Name,
                CategoryPath = DraftCategory.Path
            };
            
            PageTitle = $"Edit {Product.Name}";
            
            IsEditingBasicInfo = false;
            _toastService.ShowSuccess("Success", "Basic information updated.");
        }
        catch (Exception ex)
        {
            // Global problem details handler will show the popup
            _logger.LogError(ex, "Failed to update basic information.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Tags Logic ---
    private void BeginEditTags()
    {
        if (Product == null) return;
        
        IsEditingTags = true;
        DraftTags.Clear();
        foreach (var tag in Product.Tags)
        {
            DraftTags.Add(tag);
        }
    }

    private void CancelEditTags()
    {
        IsEditingTags = false;
    }

    private async Task SaveTagsAsync()
    {
        if (Product == null || IsLoading) return;
        
        try
        {
            IsLoading = true;
            
            var command = new UpdateProductBasicInfoCommand(
                Name: Product.Name,
                Description: Product.Description,
                BaseSku: Product.BaseSku,
                CategoryId: Product.CategoryId,
                Tags: DraftTags.ToList()
            );

            await _updateProductApi.UpdateBasicInformation(Product.Id, command);

            Product = Product with { Tags = DraftTags.ToList() };

            IsEditingTags = false;
            _toastService.ShowSuccess("Success", "Tags updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tags.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Base Logistics Logic ---
    private void BeginEditBaseLogistics()
    {
        if (Product == null) return;
        
        IsEditingBaseLogistics = true;
        DraftBaseCostAmount = Product.BaseCostAmount;
        DraftBasePriceAmount = Product.BasePriceAmount;
        DraftIsPhysicalProduct = Product.BaseWeightAmount.HasValue;
        DraftBaseWeightValue = Product.BaseWeightAmount;
    }

    private void CancelEditBaseLogistics()
    {
        IsEditingBaseLogistics = false;
    }

    private async Task SaveBaseLogisticsAsync()
    {
        if (Product == null || IsLoading) return;
        
        try
        {
            IsLoading = true;
            
            var command = new UpdateProductBaseLogisticsCommand(
                BaseCost: new UpdateMoneyDto(DraftBaseCostAmount, Product.BaseCostCurrency),
                BasePrice: new UpdateMoneyDto(DraftBasePriceAmount, Product.BasePriceCurrency),
                BaseWeight: DraftIsPhysicalProduct && DraftBaseWeightValue.HasValue 
                    ? new UpdateWeightDto(DraftBaseWeightValue.Value, Product.BaseWeightCurrency) 
                    : null
            );

            await _updateProductApi.UpdateBaseLogistics(Product.Id, command);

            // Update read model
            Product = Product with 
            { 
                BaseCostAmount = command.BaseCost.Amount,
                BasePriceAmount = command.BasePrice.Amount,
                BaseWeightAmount = command.BaseWeight?.Value
            };

            IsEditingBaseLogistics = false;
            _toastService.ShowSuccess("Success", "Base logistics updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update base logistics.");
            _toastService.ShowError("Error", "Failed to update base logistics.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ToggleHasVariantsAsync(bool newHasVariantsState)
    {
        if (Product == null || IsLoading) return;
        if (Product.HasVariants == newHasVariantsState) return;

        bool isConfirmed = false;

        if (newHasVariantsState)
        {
            // Turning ON variations
            isConfirmed = await _dialogService.ShowConfirmationAsync(
                "Enable Variations?",
                "This will convert the product to use a Variant Matrix. You will need to define Option Axes (like Size or Color) and generate variants. The current stock-on-hand tied to the base product will be managed by the variants instead.",
                "Yes, enable variations",
                "Cancel");
        }
        else
        {
            // Turning OFF variations
            isConfirmed = await _dialogService.ShowConfirmationAsync(
                "Disable Variations?",
                "Turning off variations will permanently delete all existing Options and generated Variants for this product. You will return to managing a single stock value for the base product. Are you sure you want to proceed?",
                "Yes, delete variations",
                "Cancel");
        }

        if (!isConfirmed)
        {
            // The UI binding might have already flipped the visual switch since it's TwoWay or triggered by a click.
            // By raising property changed on Product, we force the UI to revert if it was bound directly,
            // or we just do nothing and let the View handle the revert if it was a tentative state.
            // Usually, triggering a PropertyChanged on the source is enough.
            OnPropertyChanged(nameof(Product)); 
            return;
        }

        try
        {
            IsLoading = true;
            await _updateProductApi.ToggleVariants(Product.Id, new ToggleProductVariantsCommand(newHasVariantsState));
            
            // Optimistic update of the read model
            Product = Product with { HasVariants = newHasVariantsState };
            
            if (newHasVariantsState)
            {
                _toastService.ShowSuccess("Variations Enabled", "You can now add Options and generate the Variant Matrix.");
                // Ensure options list is visually reset if needed
                DraftOptions.Clear(); 
            }
            else
            {
                _toastService.ShowSuccess("Variations Disabled", "All variations and options have been removed.");
                // Ensure options and variants lists are visually cleared
                DraftOptions.Clear();
                DraftVariants.Clear();
                
                // Refresh product to load the new Default Variant generated by the backend
                await LoadProductAsync(Product.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle product variants. Targeting state: {State}", newHasVariantsState);
            _toastService.ShowError("Error", "Failed to toggle variations. Please try again.");
            OnPropertyChanged(nameof(Product)); // Force UI to revert the switch
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Default Variant Logistics Logic ---
    private void BeginEditDefaultVariantLogistics()
    {
        if (Product == null || Product.HasVariants) return;
        
        IsEditingDefaultVariantLogistics = true;

        var defaultVariant = Product.Variants.FirstOrDefault();

        // If a default variant exists, use its values, otherwise fallback to base logistics
        DraftDefaultVariantSku = defaultVariant?.Sku ?? Product.BaseSku;
        DraftDefaultVariantCostAmount = defaultVariant?.CostAmount ?? Product.BaseCostAmount;
        DraftDefaultVariantPriceAmount = defaultVariant?.PriceAmount ?? Product.BasePriceAmount;
        DraftDefaultVariantWeightValue = defaultVariant?.WeightAmount ?? Product.BaseWeightAmount;
        DraftDefaultVariantStockOnHand = defaultVariant?.StockOnHand ?? Product.StockOnHand;
    }

    private void CancelEditDefaultVariantLogistics()
    {
        IsEditingDefaultVariantLogistics = false;
    }

    private async Task SaveDefaultVariantLogisticsAsync()
    {
        if (Product == null || IsLoading || Product.HasVariants) return;
        
        try
        {
            IsLoading = true;
            
            var command = new UpdateDefaultVariantLogisticsCommand(
                ProductId: Product.Id,
                Sku: DraftDefaultVariantSku,
                Cost: new UpdateMoneyDto(DraftDefaultVariantCostAmount, Product.BaseCostCurrency),
                Price: new UpdateMoneyDto(DraftDefaultVariantPriceAmount, Product.BasePriceCurrency),
                Weight: DraftIsPhysicalProduct && DraftDefaultVariantWeightValue.HasValue 
                    ? new UpdateWeightDto(DraftDefaultVariantWeightValue.Value, Product.BaseWeightCurrency) 
                    : null,
                StockOnHand: DraftDefaultVariantStockOnHand
            );

            await _updateProductApi.UpdateDefaultVariantLogistics(Product.Id, command);

            // Fetch the updated product so the list of Variants (which holds the exact Default Variant details) is refreshed
            await LoadProductAsync(Product.Id);

            IsEditingDefaultVariantLogistics = false;
            _toastService.ShowSuccess("Success", "Inventory & Pricing updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update default variant logistics.");
            _toastService.ShowError("Error", "Failed to update inventory & pricing.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Specifications Logic ---
    private async Task BeginEditSpecificationsAsync()
    {
        if (Product == null) return;
        
        IsEditingSpecifications = true;
        DraftSpecs.Clear();
        
        try
        {
            IsLoading = true;
            
            // 1. Fetch category spec definitions
            var category = await _productCategoryLookupApi.GetCategoryById(Product.CategoryId, includeParentSpecs: true);
            
            // 2. Build the Draft UI Models mapping existing values
            foreach (var spec in category.AllSpecifications)
            {
                var existingValue = Product.Specifications.FirstOrDefault(s => s.Key == spec.Key)?.Value;
                
                var vm = new SpecFieldViewModel
                {
                    Key = spec.Key,
                    Label = spec.Label,
                    Type = spec.Type,
                    IsRequired = spec.IsRequired,
                    Options = new System.Collections.ObjectModel.ObservableCollection<string>(spec.Options),
                    Value = existingValue ?? string.Empty
                };
                DraftSpecs.Add(vm);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load category specifications.");
            _toastService.ShowError("Error", "Could not load specification options for this category.");
            IsEditingSpecifications = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CancelEditSpecifications()
    {
        IsEditingSpecifications = false;
    }

    private async Task SaveSpecificationsAsync()
    {
        if (Product == null || IsLoading) return;

        var specsDictionary = new System.Collections.Generic.Dictionary<string, string>();
        
        // 1. Validate
        foreach (var field in DraftSpecs)
        {
            if (!string.IsNullOrWhiteSpace(field.Value))
            {
                specsDictionary[field.Key] = field.Value;
            }
            else if (field.IsRequired)
            {
                _toastService.ShowError("Validation Error", $"The specification '{field.Label}' is required.");
                return;
            }
        }

        try
        {
            IsLoading = true;
            
            var command = new UpdateProductSpecificationsCommand(specsDictionary);
            await _updateProductApi.UpdateSpecifications(Product.Id, command);

            // Optimistic sync
            var updatedReadSpecs = new System.Collections.Generic.List<ProductSpecificationDto>();
            foreach(var kvp in specsDictionary)
            {
                var label = DraftSpecs.First(s => s.Key == kvp.Key).Label;
                updatedReadSpecs.Add(new ProductSpecificationDto { Key = kvp.Key, Label = label, Value = kvp.Value });
            }

            Product = Product with { Specifications = updatedReadSpecs };

            IsEditingSpecifications = false;
            _toastService.ShowSuccess("Success", "Specifications updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update specifications.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Options Logic ---
    private void BeginEditOptions()
    {
        if (Product == null) return;
        
        IsEditingOptions = true;
        DraftOptions.Clear();
        DraftOptionName = string.Empty;
        DraftOptionValues.Clear();

        foreach (var option in Product.Options)
        {
            DraftOptions.Add(new ProductOptionModel
            {
                Name = option.Name,
                Values = new ObservableCollection<string>(option.Values)
            });
        }
    }

    private void CancelEditOptions()
    {
        IsEditingOptions = false;
    }

    private async Task SaveOptionsAsync()
    {
        if (Product == null || IsLoading) return;
        
        if (DraftOptions.Count == 0)
        {
            _toastService.ShowError("Validation", "You must add at least one option axis (e.g., Size or Color) if the product has variations.");
            return;
        }

        try
        {
            IsLoading = true;
            
            var optionsDtoList = DraftOptions.Select((o, i) => new UpdateProductOptionDto(
                Name: o.Name,
                DisplayOrder: i,
                Values: o.Values.ToList()
            )).ToList();

            var command = new UpdateProductOptionsCommand(optionsDtoList);
            await _updateProductApi.UpdateOptions(Product.Id, command);

            // Optimistic sync
            var updatedReadOptions = new System.Collections.Generic.List<ProductOptionDetailsDto>();
            foreach (var opt in optionsDtoList)
            {
                updatedReadOptions.Add(new ProductOptionDetailsDto 
                { 
                    Name = opt.Name, 
                    DisplayOrder = opt.DisplayOrder, 
                    Values = opt.Values 
                });
            }

            Product = Product with { Options = updatedReadOptions };

            IsEditingOptions = false;
            _toastService.ShowSuccess("Success", "Options updated successfully.");

            // Prompt to generate variants
            var doGenerate = await _dialogService.ShowConfirmationAsync(
                "Generate Variant Matrix?",
                "Do you want to clear existing variants (if any) and generate a new grid based on these updated options? You will still need to save the Variants section afterward.",
                "Generate Grid",
                "Not Now");

            if (doGenerate)
            {
                GenerateDraftVariantsFromOptions();
                IsEditingVariants = true; // Automatically open the variants edit mode
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update options.");
            _toastService.ShowError("Error", "Failed to update options.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void GenerateDraftVariantsFromOptions()
    {
        DraftVariants.Clear();
        if (Product?.Options == null || Product.Options.Count == 0) return;

        var combinations = GenerateCombinations(Product.Options);

        foreach (var combo in combinations)
        {
            DraftVariants.Add(new EditVariantDraftModel
            {
                Id = Guid.NewGuid(), // Will be ignored by backend on create/update if treated as new, or we map to existing if attributes match
                Attributes = combo.Select(c => new CreateVariantAttributeDto(c.Name, c.Value)).ToList(),
                DisplayAttributes = string.Join(" / ", combo.Select(c => c.Value)),
                CostAmount = (double)Product.BaseCostAmount, // Inherit base logistics
                PriceAmount = (double)Product.BasePriceAmount,
                WeightValue = Product.BaseWeightAmount.HasValue ? (double)Product.BaseWeightAmount.Value : null,
                StockOnHand = 0,
                IsActive = true
            });
        }
    }

    private List<List<ProductVariantAttributeDto>> GenerateCombinations(List<ProductOptionDetailsDto> options)
    {
        var combinations = new List<List<ProductVariantAttributeDto>>();
        GenerateCombinationsRecursive(options, 0, new List<ProductVariantAttributeDto>(), combinations);
        return combinations;
    }

    private void GenerateCombinationsRecursive(List<ProductOptionDetailsDto> options, int optionIndex, List<ProductVariantAttributeDto> currentCombo, List<List<ProductVariantAttributeDto>> allCombinations)
    {
        if (optionIndex == options.Count)
        {
            allCombinations.Add(new List<ProductVariantAttributeDto>(currentCombo));
            return;
        }

        var currentOption = options[optionIndex];
        foreach (var value in currentOption.Values)
        {
            currentCombo.Add(new ProductVariantAttributeDto { Name = currentOption.Name, Value = value });
            GenerateCombinationsRecursive(options, optionIndex + 1, currentCombo, allCombinations);
            currentCombo.RemoveAt(currentCombo.Count - 1); // Backtrack
        }
    }

    private void AddOption()
    {
        if (string.IsNullOrWhiteSpace(DraftOptionName))
        {
            _toastService.ShowError("Validation", "Please enter an option name.");
            return;
        }

        if (DraftOptionValues.Count == 0)
        {
            _toastService.ShowError("Validation", "Please enter at least one option value.");
            return;
        }

        if (DraftOptions.Any(o => o.Name.Equals(DraftOptionName, StringComparison.OrdinalIgnoreCase)))
        {
            _toastService.ShowError("Validation", "An option with this name already exists.");
            return;
        }

        if (DraftOptions.Count >= 3)
        {
            _toastService.ShowError("Validation", "You can only add up to 3 options.");
            return;
        }

        var newOption = new ProductOptionModel
        {
            Name = DraftOptionName.Trim(),
            Values = new System.Collections.ObjectModel.ObservableCollection<string>(DraftOptionValues)
        };

        DraftOptions.Add(newOption);

        DraftOptionName = string.Empty;
        DraftOptionValues.Clear();
    }

    private void RemoveOption(ProductOptionModel? option)
    {
        if (option != null)
        {
            DraftOptions.Remove(option);
        }
    }

    public async Task<Onyx.Oms.Client.Desktop.Shared.Models.PagedResult<ProductCategoryDto>> FetchCategoriesAsync(string searchTerm, int page, int pageSize, System.Threading.CancellationToken token = default)
    {
        try
        {
            return await _productCategoryLookupApi.SearchCategories(page, pageSize, searchTerm, null, null, true, null, true);
        }
        catch
        {
            return new Onyx.Oms.Client.Desktop.Shared.Models.PagedResult<ProductCategoryDto>() { Items = new(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }
    }

    // --- Variants Logic ---
    private void BeginEditVariants()
    {
        if (Product == null) return;
        
        IsEditingVariants = true;
        DraftVariants.Clear();

        foreach (var variant in Product.Variants)
        {
            DraftVariants.Add(new EditVariantDraftModel
            {
                Id = variant.Id,
                Sku = variant.Sku,
                Attributes = variant.Attributes.Select(a => new CreateVariantAttributeDto(a.Name, a.Value)).ToList(),
                DisplayAttributes = string.Join(" / ", variant.Attributes.Select(a => a.Value)),
                CostAmount = (double)variant.CostAmount,
                PriceAmount = (double)variant.PriceAmount,
                WeightValue = variant.WeightAmount.HasValue ? (double)variant.WeightAmount.Value : null,
                StockOnHand = variant.StockOnHand,
                IsActive = variant.IsActive
            });
        }
    }

    private void CancelEditVariants()
    {
        IsEditingVariants = false;
    }

    private async Task SaveVariantsAsync()
    {
        if (Product == null || IsLoading) return;
        
        try
        {
            IsLoading = true;
            
            var variantsDtoList = DraftVariants.Select(v => new UpdateProductVariantDto(
                Id: v.Id,
                Sku: v.Sku,
                Attributes: v.Attributes,
                CostAmount: (decimal)v.CostAmount,
                PriceAmount: (decimal)v.PriceAmount,
                WeightAmount: v.WeightValue.HasValue ? (decimal)v.WeightValue.Value : null,
                StockOnHand: v.StockOnHand,
                IsActive: v.IsActive
            )).ToList();

            var command = new UpdateProductVariantsCommand(variantsDtoList);
            await _updateProductApi.UpdateVariants(Product.Id, command);

            // Fetch the entire product instead of optimistic sync to ensure newly generated Variant IDs are synced correctly
            await LoadProductAsync(Product.Id);

            IsEditingVariants = false;
            _toastService.ShowSuccess("Success", "Variant Matrix updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update variants.");
            _toastService.ShowError("Error", "Failed to update Variant Matrix.");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class EditVariantDraftModel : ObservableObject
{
    public Guid Id { get; init; }
    public List<CreateVariantAttributeDto> Attributes { get; init; } = new();
    public string DisplayAttributes { get; init; } = string.Empty;
    
    private string _sku = string.Empty;
    public string Sku { get => _sku; set => SetProperty(ref _sku, value); }
    
    private double _costAmount;
    public double CostAmount { get => _costAmount; set => SetProperty(ref _costAmount, value); }
    
    private double _priceAmount;
    public double PriceAmount { get => _priceAmount; set => SetProperty(ref _priceAmount, value); }
    
    private double? _weightValue;
    public double? WeightValue { get => _weightValue; set => SetProperty(ref _weightValue, value); }
    
    private int _stockOnHand;
    public int StockOnHand { get => _stockOnHand; set => SetProperty(ref _stockOnHand, value); }

    private bool _isActive;
    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
}