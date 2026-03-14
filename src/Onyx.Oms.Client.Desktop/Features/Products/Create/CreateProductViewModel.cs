using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Onyx.Oms.Client.Desktop.Features.Products.Create
{
    public partial class CreateProductViewModel : ObservableObject, INavigationAware
    {
        private readonly IProductsApi _productsApi;
        private readonly IProductCategoryLookupApi _productCategoryLookupApi;
        private readonly ITenantProfileService _tenantProfileService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastService;
        private readonly IFileService _fileService;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public CreateProductDraftModel Draft { get; } = new();

        private ProductCategoryDto? _selectedCategory;
        public ProductCategoryDto? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if(SetProperty(ref _selectedCategory, value))
                {
                    if(value != null)
                    {
                        Draft.CategoryId = value.Id;
                        _ = LoadCategorySpecifcations(value.Id);
                    }
                    else
                    {
                        Draft.CategoryId = Guid.Empty;
                        DynamicSpecs.Clear();
                    }
                }
            }
        }

        public string BaseCurrency => _tenantProfileService.Profile?.BaseCurrency ?? "LKR";
        public string BaseWeightUnit => _tenantProfileService.Profile?.WeightUnit ?? "kg";

        private bool _isPhysicalProduct = true;
        public bool IsPhysicalProduct
        {
            get => _isPhysicalProduct;
            set
            {
                if(SetProperty(ref _isPhysicalProduct, value))
                {
                    if (!value)
                        Draft.BaseWeightAmount = null;
                }
            }
        }

        private bool _hasVariants = false;
        public bool HasVariants
        {
            get => _hasVariants;
            set
            {
                if(SetProperty(ref _hasVariants, value))
                {
                    if (!value)
                    {

                    }
                }
            }
        }

        public ObservableCollection<string> Tags { get; } = new();
        public ObservableCollection<SpecFieldViewItem> DynamicSpecs { get; } = new();


        private string _draftOptionName = string.Empty;
        public string DraftOptionName
        {
            get => _draftOptionName;
            set => SetProperty(ref _draftOptionName, value);
        }
        public ObservableCollection<string> DraftOptionValues { get; } = new();
        public ObservableCollection<CreateProductOptionDraftModel> ProductOptions { get; } = new();
        public ObservableCollection<CreateProductVariantDraftModel> VariantDrafts { get; } = new();
        public ObservableCollection<CreateProductImageDraftModel> ProductImageDrafts { get; } = new();
        public ObservableCollection<ImageOptionTag> AvailableImageTags { get; } = new();

        // --- Commands ---
        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        public IRelayCommand AddOptionCommand { get; }
        public IRelayCommand RemoveOptionCommand { get; }
        public IRelayCommand GenerateMatrixCommand { get; }
        public IAsyncRelayCommand UploadImageCommand { get; }
        public IRelayCommand SetMainImageCommand { get; }
        public IAsyncRelayCommand DeleteImageCommand { get; }

        public CreateProductViewModel(
            IProductsApi productsApi,
            IProductCategoryLookupApi productCategoryLookupApi,
            ITenantProfileService tenantProfileService,
            IDialogService dialogService,
            INavigationService navigationService,
            IToastService toastService,
            IFileService fileService)
        {
            _productsApi = productsApi;
            _productCategoryLookupApi = productCategoryLookupApi;
            _tenantProfileService = tenantProfileService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _toastService = toastService;
            _fileService = fileService;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveOptionCommand = new RelayCommand<CreateProductOptionDraftModel>(RemoveOption);
            GenerateMatrixCommand = new RelayCommand(GenerateMatrix);
            UploadImageCommand = new AsyncRelayCommand(UploadImageAsync);
            SetMainImageCommand = new RelayCommand<CreateProductImageDraftModel>(SetMainImage);
            DeleteImageCommand = new AsyncRelayCommand<CreateProductImageDraftModel>(DeleteImageAsync);
        }

        public async void OnNavigatedFrom()
        {
            
        }

        public async void OnNavigatedTo(object parameter)
        {
            
        }

        public async Task<PagedResult<ProductCategoryDto>> FetchCategoriesAsync(string searchTerm, int page, int pageSize, bool isLeafOnly, CancellationToken token = default)
        {
            try
            {
                return await _productCategoryLookupApi.SearchCategories(
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

        private async Task LoadCategorySpecifcations(Guid categoryId)
        {
            try
            {
                IsBusy = true;
                var response = await _productCategoryLookupApi.GetCategoryById(categoryId, includeParentSpecs: true);

                DynamicSpecs.Clear();
                foreach (var spec in response.AllSpecifications)
                {
                    var vi = new SpecFieldViewItem
                    {
                        Key = spec.Key,
                        Label = spec.Label,
                        Type = spec.Type,
                        IsRequired = spec.IsRequired,
                        Options = new ObservableCollection<string>(spec.Options)
                    };
                    DynamicSpecs.Add(vi);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading specifications: {ex.Message}");
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

            if(ProductOptions.Any(o => o.Name.Equals(DraftOptionName, StringComparison.OrdinalIgnoreCase)))
            {
                _toastService.ShowWarning("Duplicate Option", "An option with this name already exists.");
                return;
            }

            if (ProductOptions.Count() >= 3)
            {
                _toastService.ShowWarning("Maximum Options Reached", "You can only have up to 3 option axes for a product.");
                return;
            }

            var optionModel = new CreateProductOptionDraftModel { Name = DraftOptionName };
            foreach (var v in valuesList)
            {
                optionModel.Values.Add(v);
            }

            ProductOptions.Add(optionModel);

            DraftOptionName = string.Empty;
            DraftOptionValues.Clear();
        }

        private void RemoveOption(CreateProductOptionDraftModel? option)
        {
            if(option != null)
            {
                ProductOptions.Remove(option);
                VariantDrafts.Clear();
                RefreshAvailableImageTags();
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

            // Cartesial product generation
            var combinations = GetCombinations(ProductOptions.ToList());
            foreach ( var combination in combinations)
            {
                var display = string.Join(" / ", combination.Select(a => a.Value));
                string? overrideSku = null;
                if (!string.IsNullOrWhiteSpace(Draft.BaseSku))
                {
                    var suffix = string.Join("-", combination.Select(a => a.Value.ToUpper().Replace(" ", "")));
                    overrideSku = $"{Draft.BaseSku}-{suffix}";
                }

                VariantDrafts.Add(new CreateProductVariantDraftModel
                {
                    Attributes = combination,
                    DisplayAttributes = display,
                    Sku = overrideSku,
                    CostAmount = (double)Draft.BaseCostAmount,
                    PriceAmount = (double)Draft.BasePriceAmount,
                    WeightValue = Draft.BaseWeightAmount != null ? (double)Draft.BaseWeightAmount : null,
                    StockOnHand = 0
                });
            }

            RefreshAvailableImageTags();
        }

        private List<List<ProductVariantAttributeDto>> GetCombinations(List<CreateProductOptionDraftModel> options)
        {
            var result = new List<List<ProductVariantAttributeDto>>();
            if(options.Count == 0)
                return result;

            void Permute(int depth, List<ProductVariantAttributeDto> current)
            {
                if(depth == options.Count)
                {
                    result.Add(new List<ProductVariantAttributeDto>(current));
                    return;
                }

                var currentOption = options[depth];
                foreach (var val in currentOption.Values)
                {
                    current.Add(new ProductVariantAttributeDto { Name = currentOption.Name, Value = val });
                    Permute(depth + 1, current);
                    current.RemoveAt(current.Count - 1);
                }
            }

            Permute(0, new List<ProductVariantAttributeDto>());
            return result;
        }

        private void RefreshAvailableImageTags()
        {
            var currentSelections = ProductImageDrafts.Select(i => i.SelectedTag).ToList();

            AvailableImageTags.Clear();

            foreach(var option in ProductOptions)
            {
                foreach (var val in option.Values)
                {
                    AvailableImageTags.Add(new ImageOptionTag(option.Name, val));
                }
            }

            foreach(var img in ProductImageDrafts)
            {
                img.SyncTags(AvailableImageTags);
            }
        }

        private async Task UploadImageAsync()
        {
            try
            {
                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };

                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");

                // WinUI 3 requirement: Bind the picker to the current Window HWND
                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file == null)
                    return;

                // Unique name
                string extension = Path.GetExtension(file.Path);
                string newFileName = $"{Guid.NewGuid()}{extension}";

                using var stream = await file.OpenStreamForReadAsync();

                // resize, write
                await _fileService.SaveImageAsync("ProductImages", newFileName, stream);

                var imageBytes = await _fileService.ReadFileAsync("ProductImages", newFileName);

                if (imageBytes != null)
                {
                    bool isFirstImage = ProductImageDrafts.Count == 0;

                    var previwImage = await CreateProductImageDraftModel.CreateAsync(newFileName, imageBytes, isFirstImage);
                    previwImage.SyncTags(AvailableImageTags);
                    ProductImageDrafts.Add(previwImage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error uploading image: {ex.Message}");
                _toastService.ShowError("Upload Failed", "An error occurred while uploading the image.");
            }
        }

        private void SetMainImage(CreateProductImageDraftModel? selectedImage)
        {
            if (selectedImage == null)
                return;

            foreach (var image in ProductImageDrafts)
            {
                image.IsMain = (image == selectedImage);
            }
        }

        private async Task DeleteImageAsync(CreateProductImageDraftModel? imageToDelete)
        {
            if (imageToDelete == null)
                return;

            try
            {
                // Delete physical file
                await _fileService.DeleteFileAsync("ProductImages", imageToDelete.FileName);

                // Remove from UI
                ProductImageDrafts.Remove(imageToDelete);

                if (imageToDelete.IsMain && ProductImageDrafts.Count > 0)
                    ProductImageDrafts[0].IsMain = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting image: {ex.Message}");
                _toastService.ShowError("Delete Failed", "An error occurred while deleting the image.");
            }
        }

        private async Task SaveAsync()
        {
            if (IsBusy)
                return;

            if(!await ValidateForm())
                return;

            try
            {
                IsBusy = true;
                var command = BuildCreateProductCommand();
                if(command == null)
                {
                    return;
                }

                var productId = await _productsApi.CreateProduct(command);
                _toastService.ShowSuccess("Success", "Product created successfully.");
                _navigationService.GoBack();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save Failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> ValidateForm()
        {
            var validationsErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(Draft.Name))
            {
                validationsErrors.Add("Name is required.");
            }
            if(Draft.CategoryId == Guid.Empty)
            {
                validationsErrors.Add("Category is required.");
            }

            foreach (var field in DynamicSpecs)
            {
                if (string.IsNullOrWhiteSpace(field.Value) && field.IsRequired)
                {
                    validationsErrors.Add($"The specification '{field.Label}' is required.");
                }
            }

            if(HasVariants && !ProductOptions.Any())
            {
                validationsErrors.Add("At least one option must be generated when 'Has Variants' is enabled.");
            }

            if(HasVariants && !VariantDrafts.Any())
            {
                validationsErrors.Add("At least one variant must be generated when 'Has Variants' is enabled.");
            }

            if (validationsErrors.Count > 0)
                await _dialogService.ShowValidationErrorsAsync("Validation Failed", validationsErrors);

            return validationsErrors.Count == 0;
        }

        private CreateProductDto? BuildCreateProductCommand()
        {
            var specDictionary = new Dictionary<string, string>();
            foreach(var field in DynamicSpecs)
            {
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    specDictionary[field.Key] = field.Value;
                }
                else if (field.IsRequired)
                {
                    _toastService.ShowError("Validation Error", $"The specification '{field.Label}' is required.");
                    IsBusy = false;
                    return null;
                }
            }

            WeightDto? baseWeight = null;
            if (IsPhysicalProduct)
            {
                decimal weightValue = Draft.BaseWeightAmount ?? 0;
                baseWeight = new WeightDto { Value = weightValue, Unit = BaseWeightUnit };
            }

            int? stockOnHand = HasVariants ? null : Draft.BaseStockOnHand;

            var optionDto = HasVariants && ProductOptions.Any()
                ? ProductOptions.Select(o => new ProductOptionDto { Name = o.Name, Values = o.Values.ToList() }).ToList()
                : new List<ProductOptionDto>();

            var variantsDto = HasVariants && VariantDrafts.Any()
                ? VariantDrafts.Select(v => new CreateProductVariantDto
                {
                    Sku = v.Sku,
                    Attributes = v.Attributes,
                    Cost = new MoneyDto { Amount = (decimal)v.CostAmount, Currency = BaseCurrency },
                    Price = new MoneyDto { Amount = (decimal)v.PriceAmount, Currency = BaseCurrency },
                    Weight = v.WeightValue != null ? new WeightDto { Value = (decimal)v.WeightValue, Unit = BaseWeightUnit } : null,
                    StockOnHand = v.StockOnHand,
                }).ToList()
                : new List<CreateProductVariantDto>();

            var imageDtos = ProductImageDrafts.Select((img, index) => new CreateProductImageDto
            {
                Url = img.FileName,
                DisplayOrder = index,
                IsMain = img.IsMain,
                OptionName = img.SelectedTag?.OptionName,
                OptionValue = img.SelectedTag?.OptionValue,
            }).ToList();

            return new CreateProductDto
            {
                Name = Draft.Name,
                BaseSku = string.IsNullOrWhiteSpace(Draft.BaseSku) ? null : Draft.BaseSku,
                Description = string.IsNullOrWhiteSpace(Draft.Description) ? null : Draft.Description,
                CategoryId = Draft.CategoryId,
                BaseCost = new MoneyDto { Amount = Draft.BaseCostAmount, Currency = BaseCurrency },
                BasePrice = new MoneyDto { Amount = Draft.BasePriceAmount, Currency = BaseCurrency },
                BaseWeight = baseWeight,
                BaseStockOnHand = stockOnHand,
                Tags = Tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToList(),
                Specifications = specDictionary,
                HasVariants = HasVariants,
                Options = optionDto,
                Variants = variantsDto,
                Images = imageDtos,
            };
        }

        private async Task CancelAsync()
        {
            _navigationService.GoBack();
        }
    }
}
