using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public partial class ProductDetailsViewModel : ObservableObject, INavigationAware
{
    private readonly IGetProductDetailsApi _productApi;
    private readonly ILogger<ProductDetailsViewModel> _logger;
    private readonly INavigationService _navigationService;
    private readonly IFileService _fileService;

    public ProductDetailsViewModel(
        IGetProductDetailsApi productApi,
        ILogger<ProductDetailsViewModel> logger,
        INavigationService navigationService,
        IFileService fileService)
    {
        _productApi = productApi;
        _logger = logger;
        _navigationService = navigationService;
        _fileService = fileService;
        
        GoBackCommand = new RelayCommand(GoBack);
        OptionSelectedCommand = new RelayCommand<ProductOptionValueDisplayModel>(OnOptionSelected);
        ImageSelectedCommand = new RelayCommand<ProductImageDisplayModel>(OnImageSelected);
    }

    private ProductDetailsDto? _product;
    public ProductDetailsDto? Product
    {
        get => _product;
        set 
        {
            if (SetProperty(ref _product, value))
            {
                OnPropertyChanged(nameof(HasSpecifications));
                OnPropertyChanged(nameof(HasDescription));
                OnPropertyChanged(nameof(Specifications));
                OnPropertyChanged(nameof(HasVariants));
                OnPropertyChanged(nameof(Variants));
            }
        }
    }

    public bool HasSpecifications => Product?.Specifications?.Any() == true;
    public bool HasDescription => !string.IsNullOrWhiteSpace(Product?.Description);
    public bool HasVariants => Product?.Variants?.Any() == true;

    public System.Collections.Generic.IEnumerable<ProductSpecificationModel> Specifications 
        => Product?.Specifications?.Select(s => new ProductSpecificationModel(s.Label, s.Value)) ?? Array.Empty<ProductSpecificationModel>();

    public System.Collections.Generic.IEnumerable<ProductVariantDisplayModel> Variants 
        => Product?.Variants?.Select(v => new ProductVariantDisplayModel(v)) ?? Array.Empty<ProductVariantDisplayModel>();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private ObservableCollection<ProductImageDisplayModel> _displayImages = new();
    public ObservableCollection<ProductImageDisplayModel> DisplayImages
    {
        get => _displayImages;
        set => SetProperty(ref _displayImages, value);
    }

    private bool _isSelectingOption;

    private int _selectedImageIndex = 0;
    public int SelectedImageIndex
    {
        get => _selectedImageIndex;
        set 
        {
            if (SetProperty(ref _selectedImageIndex, value))
            {
                if (!_isSelectingOption && value >= 0 && value < DisplayImages.Count)
                {
                    OnImageSelected(DisplayImages[value]);
                }
            }
        }
    }

    private ObservableCollection<ProductOptionDisplayModel> _options = new();
    public ObservableCollection<ProductOptionDisplayModel> Options
    {
        get => _options;
        set => SetProperty(ref _options, value);
    }

    public IRelayCommand GoBackCommand { get; }
    public IRelayCommand<ProductOptionValueDisplayModel> OptionSelectedCommand { get; }
    public IRelayCommand<ProductImageDisplayModel> ImageSelectedCommand { get; }

    private void OnOptionSelected(ProductOptionValueDisplayModel? selectedValue)
    {
        if (selectedValue == null) return;
        
        _isSelectingOption = true;
        try
        {
            // Deselect all other options in this group
            var group = Options.FirstOrDefault(o => o.Name == selectedValue.OptionName);
            if (group != null)
            {
                foreach (var val in group.Values)
                {
                    if (val != selectedValue)
                    {
                        val.IsSelected = false;
                    }
                }
                // Ensure the clicked one stays selected if it was toggled off
                selectedValue.IsSelected = true;
            }

            var matchingImage = DisplayImages.FirstOrDefault(i => i.Dto.OptionValue == selectedValue.Value);
            if (matchingImage != null)
            {
                SelectedImageIndex = DisplayImages.IndexOf(matchingImage);
            }
        }
        finally
        {
            _isSelectingOption = false;
        }
    }

    private void OnImageSelected(ProductImageDisplayModel? selectedImage)
    {
        if (selectedImage == null) return;

        var optionGroup = Options.FirstOrDefault(o => o.Name == selectedImage.Dto.OptionName);
        if (optionGroup != null)
        {
            foreach (var val in optionGroup.Values)
            {
                if (val.Value == selectedImage.Dto.OptionValue)
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
            Product = await _productApi.GetProductById(id);
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
        if (Product?.Options == null) return;

        foreach (var optDto in Product.Options.OrderBy(o => o.DisplayOrder))
        {
            var displayModel = new ProductOptionDisplayModel { Name = optDto.Name };
            foreach (var val in optDto.Values)
            {
                displayModel.Values.Add(new ProductOptionValueDisplayModel 
                { 
                    Value = val, 
                    OptionName = optDto.Name 
                });
            }
            Options.Add(displayModel);
        }
    }

    private async Task LoadImageSourcesAsync()
    {
        if (Product == null || !Product.Images.Any()) return;

        DisplayImages.Clear();
        foreach (var imgDto in Product.Images.OrderBy(i => i.DisplayOrder))
        {
            var displayModel = new ProductImageDisplayModel { Dto = imgDto };

            if (!string.IsNullOrWhiteSpace(imgDto.Url))
            {
                try
                {
                    var imageBytes = await _fileService.ReadFileAsync("ProductImages", imgDto.Url);
                    if (imageBytes != null)
                    {
                        using var stream = new MemoryStream(imageBytes);
                        using var randomAccessStream = stream.AsRandomAccessStream();
                        var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                        await bitmap.SetSourceAsync(randomAccessStream);
                        displayModel.ImageSource = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading image for product {ProductId}: {Url}", Product.Id, imgDto.Url);
                }
            }

            DisplayImages.Add(displayModel);
            if (imgDto.IsMain)
            {
                SelectedImageIndex = DisplayImages.Count - 1;
            }
        }
    }
}

public class ProductImageDisplayModel
{
    public ProductImageDto Dto { get; set; } = null!;
    public Microsoft.UI.Xaml.Media.Imaging.BitmapImage? ImageSource { get; set; }
}

public class ProductSpecificationModel
{
    public string Label { get; }
    public string Value { get; }

    public ProductSpecificationModel(string label, string value)
    {
        Label = label;
        Value = value;
    }
}

public class ProductOptionDisplayModel : ObservableObject
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public ObservableCollection<ProductOptionValueDisplayModel> Values { get; } = new();
}

public class ProductOptionValueDisplayModel : ObservableObject
{
    private string _value = string.Empty;
    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    private string _optionName = string.Empty;
    public string OptionName
    {
        get => _optionName;
        set => SetProperty(ref _optionName, value);
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public class ProductVariantDisplayModel
{
    public ProductVariantDto Dto { get; }
    public string Description => string.Join(" / ", Dto.Attributes.Select(a => a.Value));
    public int AvailableQuantity => Dto.StockOnHand - Dto.ReservedQuantity;

    public ProductVariantDisplayModel(ProductVariantDto dto)
    {
        Dto = dto;
    }
}
