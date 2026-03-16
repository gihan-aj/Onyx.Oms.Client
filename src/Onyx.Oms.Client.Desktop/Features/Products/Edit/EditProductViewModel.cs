using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public IRelayCommand GoBackCommand { get; }
        public IAsyncRelayCommand SaveBasicInfoCommand { get; }
        public IAsyncRelayCommand SaveTagsCommand { get; }

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
            if(_productDetails != null)
            {
                PageTitle = $"Edit {_productDetails.Name}";
                BasicInfo = new EditProductBasicInfoViewModel(_productDetails, OnCategoryChanged, _dialogService);
                Tags = new EditProductTagsViewModel(_productDetails);
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
            var response = await LoadCategorySpecificationsAsync(category.Id);
        }

        private async Task<ObservableCollection<SpecFieldViewItem>> LoadCategorySpecificationsAsync(Guid categoryId)
        {
            var specifications = new ObservableCollection<SpecFieldViewItem>();
            try
            {
                IsBusy = true;
                var response = await _productCategoryLookupApi.GetCategoryById(categoryId, includeParentSpecs: true);

                //DynamicSpecs.Clear();
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
                    specifications.Add(vi);
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
    }
}
