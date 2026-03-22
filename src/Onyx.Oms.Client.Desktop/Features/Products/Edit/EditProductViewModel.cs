using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products.Edit
{
    public partial class EditProductViewModel : ObservableObject, INavigationAware
    {
        private readonly IProductsApi _productsApi;
        private readonly IProductCategoryLookupApi _productCategoryLookupApi;
        private readonly ITenantProfileService _tenantProfileService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastService;
        private readonly IFileService _fileService;
        private readonly ILogger<EditProductViewModel> _logger;

        private ProductDetailsDto? _productDetails;
        private List<SpecDefinition>? _categorySpecDefinitions;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }


        private string _pageTitle = "Edit Product";
        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        private EditProductBasicInfoViewModel? _basicInfo;
        public EditProductBasicInfoViewModel? BasicInfo
        {
            get => _basicInfo;
            set => SetProperty(ref _basicInfo, value);
        }

        private EditProductTagsViewModel? _tags;
        public EditProductTagsViewModel? Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        private EditProductSpecificationsViewModel? _specifications;
        public EditProductSpecificationsViewModel? Specifications
        {
            get => _specifications;
            set => SetProperty(ref _specifications, value);
        }

        private bool _hasVariants;
        public bool HasVariants
        {
            get => _hasVariants;
            set
            {
                if (SetProperty(ref _hasVariants, value))
                {
                    if (BaseLogistics != null)
                    {
                        BaseLogistics.HasVariants = value;                    
                    }

                    if(Options != null)
                    {
                        Options.HasVariants = value;
                    }
                }
            }
        }

        private EditProductBaseLogisticsViewModel? _baseLogistics;
        public EditProductBaseLogisticsViewModel? BaseLogistics
        {
            get => _baseLogistics;
            set => SetProperty(ref _baseLogistics, value);
        }

        private EditProductOptionsViewModel? _options;
        public EditProductOptionsViewModel? Options
        {
            get => _options;
            set => SetProperty(ref _options, value);
        }

        public IRelayCommand GoBackCommand { get; }
        public IAsyncRelayCommand SaveBasicInfoCommand { get; }
        public IAsyncRelayCommand SaveTagsCommand { get; }
        public IAsyncRelayCommand SaveSpecificationsCommand { get; }
        public IAsyncRelayCommand ToggleVariantsCommand { get; }
        public IAsyncRelayCommand SaveBaseLogisticsCommand { get; }
        public IAsyncRelayCommand SaveOptionsCommand { get; }

        public EditProductViewModel(
            IProductsApi productsApi,
            IProductCategoryLookupApi productCategoryLookupApi,
            ITenantProfileService tenantProfileService,
            IDialogService dialogService,
            INavigationService navigationService,
            IToastService toastService,
            IFileService fileService,
            ILogger<EditProductViewModel> logger)
        {
            _productsApi = productsApi;
            _productCategoryLookupApi = productCategoryLookupApi;
            _tenantProfileService = tenantProfileService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _toastService = toastService;
            _fileService = fileService;
            _logger = logger;

            GoBackCommand = new RelayCommand(GoBack);
            SaveBasicInfoCommand = new AsyncRelayCommand(SaveBasicInfoAsync);
            SaveTagsCommand = new AsyncRelayCommand(SaveTagsAsync);
            SaveSpecificationsCommand = new AsyncRelayCommand(SaveSpecificationsAsync);
            ToggleVariantsCommand = new AsyncRelayCommand<bool>(ToggleVariantsAsync);
            SaveBaseLogisticsCommand = new AsyncRelayCommand(SaveBaseLogisticsAsync);
            SaveOptionsCommand = new AsyncRelayCommand(SaveOptionsAsync);
        }

        public async void OnNavigatedFrom()
        {

        }

        public async void OnNavigatedTo(object parameter)
        {
            if (parameter is Guid productId)
            {
                await InitializeAsync(productId);
            }
            else if (parameter is string productIdString && Guid.TryParse(productIdString, out var parsedId))
            {
                await InitializeAsync(parsedId);
            }
        }

        private void GoBack()
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
            else
            {
                _navigationService.NavigateTo("Onyx.Oms.Client.Desktop.Features.Products.List.ProductsPage");
            }
        }

        private async Task InitializeAsync(Guid productId)
        {
            await LoadProductAsync(productId);
            if (_productDetails != null)
            {
                PageTitle = $"Edit {_productDetails.Name}";
                BasicInfo = new EditProductBasicInfoViewModel(_productDetails, OnCategoryChanged, _dialogService);
                Tags = new EditProductTagsViewModel(_productDetails);
                _categorySpecDefinitions = await LoadCategorySpecificationsAsync(_productDetails.CategoryId);
                if (_categorySpecDefinitions != null)
                {
                    Specifications = new EditProductSpecificationsViewModel(_categorySpecDefinitions, _productDetails.Specifications, _dialogService);
                }
                HasVariants = _productDetails.HasVariants;
                BaseLogistics = new EditProductBaseLogisticsViewModel(_productDetails, _tenantProfileService);
                Options = new EditProductOptionsViewModel(_productDetails, _toastService, _dialogService);
            }

        }

        private async Task LoadProductAsync(Guid id)
        {
            try
            {
                IsBusy = true;
                _productDetails = await _productsApi.GetProductById(id);
                //LoadOptions();
                //await LoadImageSourcesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load product details for {ProductId}", id);
            }
            finally
            {
                IsBusy = false;
            }
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

        private async void OnCategoryChanged(ProductCategoryDto category)
        {
            _categorySpecDefinitions = await LoadCategorySpecificationsAsync(category.Id);
            if (Specifications != null)
                Specifications.RebuildFields(_categorySpecDefinitions);
        }

        private async Task<List<SpecDefinition>> LoadCategorySpecificationsAsync(Guid categoryId)
        {
            var specifications = new List<SpecDefinition>();
            try
            {
                IsBusy = true;
                var response = await _productCategoryLookupApi.GetCategoryById(categoryId, includeParentSpecs: true);

                foreach (var spec in response.AllSpecifications)
                {
                    specifications.Add(spec);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading specifications: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }

            return specifications;
        }

        private async Task SaveBasicInfoAsync()
        {
            if (BasicInfo == null || _productDetails == null)
                return;

            var basicInfoDto = await BasicInfo.GetUpdateDto();
            if (basicInfoDto == null)
                return;

            try
            {
                IsBusy = true;
                await _productsApi.UpdateProductBasicInfo(_productDetails.Id, basicInfoDto);

                await InitializeAsync(_productDetails.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update basic information.");
            }
            finally
            {
                IsBusy = false;
            }

        }

        private async Task SaveTagsAsync()
        {
            if (Tags == null || _productDetails == null)
                return;

            var tagsDto = await Tags.GetUpdateDto();
            if (tagsDto == null)
                return;

            try
            {
                IsBusy = true;
                await _productsApi.UpdateProductBasicInfo(_productDetails.Id, tagsDto);

                await InitializeAsync(_productDetails.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update tags.");
            }
            finally
            {
                IsBusy = false;
            }

        }

        private async Task SaveSpecificationsAsync()
        {
            if (Specifications == null || _productDetails == null)
                return;

            var specsDto = await Specifications.GetUpdateDto(_productDetails.CategoryId);
            if (specsDto == null)
                return;

            try
            {
                IsBusy = true;
                await _productsApi.UpdateProductSpecifications(_productDetails.Id, specsDto);

                await InitializeAsync(_productDetails.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update tags.");
            }
            finally
            {
                IsBusy = false;
            }

        }

        private async Task ToggleVariantsAsync(bool newHasVariantsState)
        {
            if (_productDetails == null || IsBusy)
                return;
            if (HasVariants == newHasVariantsState)
                return;

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
                OnPropertyChanged(nameof(HasVariants));
                return;
            }

            try
            {
                IsBusy = true;
                var toggleDto = new ToggleProductVariantsDto
                {
                    Id = _productDetails.Id,
                    HasVariants = newHasVariantsState,
                };
                await _productsApi.ToggleProductVariants(_productDetails.Id, toggleDto);
                await InitializeAsync(_productDetails.Id);

                if (newHasVariantsState)
                {
                    _toastService.ShowSuccess("Variations Enabled", "You can now add Options and generate the Variant Matrix.");
                    // Ensure options list is visually reset if needed
                    //DraftOptions.Clear();
                }
                else
                {
                    _toastService.ShowSuccess("Variations Disabled", "All variations and options have been removed.");
                    // Ensure options and variants lists are visually cleared
                    //DraftOptions.Clear();
                    //DraftVariants.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle variant status.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveBaseLogisticsAsync()
        {
            if (_productDetails == null || BaseLogistics == null || IsBusy)
                return;

            try
            {
                IsBusy = true;
                var (defaultVariantDto, baseLogisticsDto) = BaseLogistics.GetUpdateDtos();
                if (baseLogisticsDto != null)
                {
                    await _productsApi.UpdateProductBaseLogistics(_productDetails.Id, baseLogisticsDto);
                }
                if (!HasVariants && defaultVariantDto != null)
                {
                    await _productsApi.UpdateDefaultVariantLogistics(_productDetails.Id, defaultVariantDto);
                }
                await InitializeAsync(_productDetails.Id);

                if (defaultVariantDto != null && baseLogisticsDto != null)
                {
                    _toastService.ShowSuccess("Logistics Updated", "Product logistics and stock were updated successfully.");
                }
                else if (baseLogisticsDto != null)
                {
                    _toastService.ShowSuccess("Base Logistics Updated", "Product base logistics were updated successfully.");
                }
                else if (defaultVariantDto != null)
                {
                    _toastService.ShowSuccess("Default Variant Logistics Updated", "Stock is updated successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product logistics and settings.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveOptionsAsync()
        {
            if (Options == null || _productDetails == null || IsBusy)
                return;
            try
            {
                IsBusy = true;
                var optionsDto = await Options.GetUpdateDto();
                if (optionsDto != null)
                {
                    await _productsApi.UpdateProductOptions(_productDetails.Id, optionsDto);
                    await InitializeAsync(_productDetails.Id);
                    _toastService.ShowSuccess("Options Updated", "Product options were updated successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product options.");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
