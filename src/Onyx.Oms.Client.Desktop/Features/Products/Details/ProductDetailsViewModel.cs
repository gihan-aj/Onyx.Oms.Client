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

            // 1. CRITICAL: Reset index to -1 BEFORE clearing the list to prevent PipsPager crash!
            SelectedImageIndex = -1;
            DisplayImages.Clear();
            foreach (var imageDto in Product.Images.OrderBy(i => i.DisplayOrder))
            {
                var displayItem = new ProductImageViewItem
                {
                    Dto = imageDto,
                };

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

                DisplayImages.Add(displayItem);
                if (imageDto.IsMain)
                {
                    SelectedImageIndex = DisplayImages.Count - 1;
                }
            }

            // 2. CRITICAL: If no image was explicitly marked as main, default to the first image (0).
            if (SelectedImageIndex == -1 && DisplayImages.Count > 0)
            {
                SelectedImageIndex = 0;
            }
        }
    }
}
