using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Features.Products.List;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products.Details
{
    public partial class ProductDetailsViewModel : ObservableObject, INavigationAware
    {
        private readonly IProductsApi _productsApi;
        private readonly IFileService _fileService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<ProductDetailsViewModel> _logger;

        private ProductDetailsDto? _product;
        public ProductDetailsDto? Product
        {
            get => _product;
            set
            {
                if(SetProperty(ref _product, value))
                {
                    OnPropertyChanged(nameof(HasSpecifications));
                    OnPropertyChanged(nameof(Specifications));
                    OnPropertyChanged(nameof(HasDescription));
                    OnPropertyChanged(nameof(HasVariants));
                    OnPropertyChanged(nameof(Variants));
                }
            }
        }

        public bool HasSpecifications => Product?.Specifications?.Any() ?? false;
        public bool HasDescription => !string.IsNullOrEmpty(Product?.Description);
        public bool HasVariants => Product?.Variants?.Any() ?? false;
        public IEnumerable<ProductSpecificationViewItem> Specifications
            => Product?.Specifications?.Select(s => new ProductSpecificationViewItem(s)) ?? Enumerable.Empty<ProductSpecificationViewItem>();
        public IEnumerable<ProductVariantViewItem> Variants
            => Product?.Variants?.Select(v => new ProductVariantViewItem(v)) ?? Enumerable.Empty<ProductVariantViewItem>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private ObservableCollection<ProductImageViewItem> _displayImages = new();
        public ObservableCollection<ProductImageViewItem> DisplayImages
        {
            get => _displayImages;
            set => SetProperty(ref _displayImages, value);
        }

        private bool _isSelectingOption;

        private int _selectedImageIndex = -1;
        public int SelectedImageIndex
        {
            get => _selectedImageIndex;
            set
            {
                if (SetProperty(ref _selectedImageIndex, value))
                {
                    if (!_isSelectingOption && value >=0 && value < DisplayImages.Count)
                    {
                        OnImageSelected(DisplayImages[value]);
                    }
                }
            }
        }

        private ObservableCollection<ProductOptionViewItem> _options = new();
        public ObservableCollection<ProductOptionViewItem> Options
        {
            get => _options;
            set => SetProperty(ref _options, value);
        }

        private ProductDetailsVariantDto? _selectedVariant;

        public decimal DisplayPriceAmount => _selectedVariant?.PriceAmount ?? Product?.BasePriceAmount ?? 0;
        public string DisplayPriceCurrency => _selectedVariant?.PriceCurrency ?? Product?.BasePriceCurrency ?? string.Empty;
        public int DisplayStockOnHand => _selectedVariant?.StockOnHand ?? Product?.StockOnHand ?? 0;
        public int DisplayReservedQuantity => _selectedVariant?.ReservedQuantity ?? Product?.ReservedQuantity ?? 0;
        public string? DisplaySku => _selectedVariant?.Sku ?? Product?.BaseSku;

        public decimal? DisplayWeightAmount => _selectedVariant?.WeightAmount ?? Product?.BaseWeightAmount;
        public string? DisplayWeightUnit => _selectedVariant?.WeightUnit ?? Product?.BaseWeightUnit;


        public IRelayCommand GoBackCommand { get; }
        public IRelayCommand<ProductOptionValueViewItem> OptionSelectedCommand { get; }
        public IRelayCommand<ProductImageViewItem> ImageSelectedCommand { get; }

        public ProductDetailsViewModel(
            IProductsApi productsApi,
            IFileService fileService,
            INavigationService navigationService,
            ILogger<ProductDetailsViewModel> logger)
        {
            _productsApi = productsApi;
            _fileService = fileService;
            _navigationService = navigationService;
            _logger = logger;

            GoBackCommand = new RelayCommand(GoBack);
            OptionSelectedCommand = new RelayCommand<ProductOptionValueViewItem>(OnOptionSelected);
            ImageSelectedCommand = new RelayCommand<ProductImageViewItem>(OnImageSelected);
        }

        public void OnNavigatedFrom()
        {
            
        }

        public async void OnNavigatedTo(object parameter)
        {
            if(parameter is Guid productId)
            {
                await LoadProductAsync(productId);
            }
            else if (parameter is string productIdString && Guid.TryParse(productIdString, out var parsedId))
            {
                await LoadProductAsync(parsedId);
            }
        }

        private void OnOptionSelected(ProductOptionValueViewItem? selectedValue)
        {
            if (selectedValue == null)
                return;

            _isSelectingOption = true;
            try
            {
                var group = Options.FirstOrDefault(o => o.Name == selectedValue.OptionName);
                if(group != null)
                {
                    foreach(var val  in group.Values)
                    {
                        if (val != selectedValue)
                            val.IsSelected = false;
                    }
                    selectedValue.IsSelected = true;
                }

                var matchingImage = DisplayImages.FirstOrDefault(i => i.Dto?.OptionValue == selectedValue.Value);
                if(matchingImage != null)
                {
                    SelectedImageIndex = DisplayImages.IndexOf(matchingImage);
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                _isSelectingOption = false;
            }
            EvaluateSelectedVariant();
        }

        private void OnImageSelected(ProductImageViewItem? selectedImage)
        {
            if (selectedImage == null) 
                return;

            var optionGroup = Options.FirstOrDefault(o => o.Name == selectedImage.Dto?.OptionName);
            if(optionGroup != null)
            {
                foreach (var val in optionGroup.Values)
                {
                    if(val.Value == selectedImage.Dto?.OptionValue)
                    {
                        val.IsSelected = true;
                    }
                    else
                    {
                        val.IsSelected = false;
                    }
                }
            }
        }

        private void EvaluateSelectedVariant()
        {
            if (Product == null || !HasVariants || Product.Variants == null)
            {
                SetSelectedVariant(null);
                return;
            }

            var selectedAttributes = new List<KeyValuePair<string, string>>();

            foreach (var optionGroup in Options)
            {
                var selectedVal = optionGroup.Values.FirstOrDefault(v => v.IsSelected);
                if (selectedVal == null)
                {
                    // Not all options have a selection yet. Fall back to base product values.
                    SetSelectedVariant(null);
                    return;
                }
                selectedAttributes.Add(new KeyValuePair<string, string>(optionGroup.Name, selectedVal.Value));
            }

            // Find the exact variant that matches ALL selected attributes
            var matchingVariant = Product.Variants.FirstOrDefault(v =>
                selectedAttributes.All(sa =>
                    v.Attributes.Any(va => va.Name == sa.Key && va.Value == sa.Value)
                )
            );

            SetSelectedVariant(matchingVariant);
        }

        private void SetSelectedVariant(ProductDetailsVariantDto? variant)
        {
            if (_selectedVariant != variant)
            {
                _selectedVariant = variant;
            }
            // Notify XAML that these values have updated!
            OnPropertyChanged(nameof(DisplayPriceAmount));
            OnPropertyChanged(nameof(DisplayPriceCurrency));
            OnPropertyChanged(nameof(DisplayStockOnHand));
            OnPropertyChanged(nameof(DisplayReservedQuantity));
            OnPropertyChanged(nameof(DisplaySku));
            OnPropertyChanged(nameof(DisplayWeightAmount));
            OnPropertyChanged(nameof(DisplayWeightUnit));
        }


        private void GoBack()
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
            else
            {
                _navigationService.NavigateTo(typeof(ProductsPage).FullName!);
            }
        }

        private async Task LoadProductAsync(Guid id)
        {
            try
            {
                IsLoading = true;
                Product = await _productsApi.GetProductById(id);
                LoadOptions();
                await LoadImageSourcesAsync();
                EvaluateSelectedVariant();
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

        private void LoadOptions()
        {
            Options.Clear();
            if (Product?.Options == null)
                return;

            foreach(var optDto in Product.Options.OrderBy(opt => opt.DisplayOrder))
            {
                var displayItem = new ProductOptionViewItem
                {
                    Name = optDto.Name,
                };
                foreach(var val in optDto.Values)
                {
                    displayItem.Values.Add(new ProductOptionValueViewItem
                    {
                        Value = val,
                        OptionName = optDto.Name,
                    });
                }
                Options.Add(displayItem);
            }
        }

        private async Task LoadImageSourcesAsync()
        {
            if (Product == null || !Product.Images.Any())
                return;

            // Build a temporary list so the UI doesn't update mid-loop!
            var tempImages = new List<ProductImageViewItem>();
            int tempSelectedIndex = -1;

            foreach (var imageDto in Product.Images.OrderBy(i => i.DisplayOrder))
            {
                var displayItem = new ProductImageViewItem { Dto = imageDto };
                if (!string.IsNullOrWhiteSpace(imageDto.Url))
                {
                    try
                    {
                        var imageBytes = await _fileService.ReadFileAsync("ProductImages", imageDto.Url);
                        if (imageBytes != null)
                        {
                            using var stream = new MemoryStream(imageBytes);
                            using var randomAccessStream = stream.AsRandomAccessStream();
                            var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                            await bitmap.SetSourceAsync(randomAccessStream);
                            displayItem.ImageSource = bitmap;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading image for product {ProductId}: {Url}", Product.Id, imageDto.Url);
                    }
                }
                tempImages.Add(displayItem);
                if (imageDto.IsMain)
                {
                    tempSelectedIndex = tempImages.Count - 1;
                }
            }
            if (tempSelectedIndex == -1 && tempImages.Count > 0)
            {
                tempSelectedIndex = 0;
            }

            // 2. Now that all awaits are done, update the UI properties synchronously
            SelectedImageIndex = tempSelectedIndex;
            DisplayImages.Clear();

            foreach (var img in tempImages)
            {
                DisplayImages.Add(img);
            }
        }
    }
}
