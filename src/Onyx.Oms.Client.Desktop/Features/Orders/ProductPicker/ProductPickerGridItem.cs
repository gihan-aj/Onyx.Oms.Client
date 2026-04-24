using Microsoft.UI.Xaml.Media.Imaging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.ProductPicker
{
    public class ProductPickerGridItem : ProductDto, INotifyPropertyChanged
    {
        private IFileService? _fileService;
        private string? _currentImageUrl;

        public string DisplayName
        {
            get
            {
                var parts = new List<string> { Name ?? "Unknown Product" };
                if (ResolvedVariant != null && ResolvedVariant.Attributes.Count > 0)
                {
                    foreach (var attr in ResolvedVariant.Attributes)
                    {
                        parts.Add(attr.Value);
                    }
                    return string.Join(" - ", parts);
                }
                return parts[0];
            }
        }

        private BitmapImage? _imageSource;
        public BitmapImage? ImageSource
        {
            get => _imageSource;
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));
                }
            }
        }
        public ObservableCollection<UiOption> UiOptions { get; set; } = new();
        public Action<ProductPickerGridItem>? OnOptionInteraction { get; set; }

        public string DisplaySku => ResolvedVariant != null ? ResolvedVariant.Sku : BaseSku;
        public string DisplayAvailableQuantity => $"Available: {(ResolvedVariant != null ? (ResolvedVariant.StockOnHand - ResolvedVariant.ReservedQuantity) : AvailableQuantity)}";
        public string? ResolvedImageUrl => _currentImageUrl ?? MainImageUrl;
        public bool ShouldShowQuantity => !HasVariants || ResolvedVariant != null;

        public Microsoft.UI.Xaml.Media.Brush QuantityForeground
        {
            get
            {
                var qty = ResolvedVariant != null ? (ResolvedVariant.StockOnHand - ResolvedVariant.ReservedQuantity) : AvailableQuantity;
                if (qty <= 0) return (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["SystemFillColorCriticalBrush"];
                if (qty < 10) return (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["SystemFillColorCautionBrush"];
                return (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["SystemFillColorSuccessBrush"];
            }
        }

        private ProductVariantDto? _resolvedVariant;
        public ProductVariantDto? ResolvedVariant
        {
            get => _resolvedVariant;
            private set
            {
                if (_resolvedVariant != value)
                {
                    _resolvedVariant = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResolvedVariant)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplaySku)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayAvailableQuantity)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShouldShowQuantity)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(QuantityForeground)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void InitializeDependencies(IFileService fileService)
        {
            _fileService = fileService;
        }

        public void EvaluateResolvedVariant()
        {
            if (!HasVariants)
            {
                ResolvedVariant = Variants?.FirstOrDefault();
                return;
            }

            var selectedDict = UiOptions
                .Where(o => o.SelectedValue != null)
                .ToDictionary(o => o.Name, o => o.SelectedValue?.Value);

            if (selectedDict.Count == Options.Count)
            {
                ResolvedVariant = Variants?.FirstOrDefault(v =>
                    v.Attributes.Count == selectedDict.Count &&
                    v.Attributes.All(a => selectedDict.TryGetValue(a.Name, out var selVal) && selVal == a.Value));
            }
            else
            {
                ResolvedVariant = null;
            }
        }

        public async Task UpdateImageAsync()
        {
            if (_fileService == null)
                return;

            string? targetImageUrl = MainImageUrl;

            if (Images != null && Images.Any())
            {
                foreach (var opt in UiOptions)
                {
                    if (opt.SelectedValue != null)
                    {
                        var matchingImage = Images.FirstOrDefault(i =>
                            i.OptionName == opt.Name &&
                            i.OptionValue == opt.SelectedValue.Value);

                        if (matchingImage != null)
                        {
                            targetImageUrl = matchingImage.Url;
                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(targetImageUrl) || targetImageUrl == _currentImageUrl)
                return;

            try
            {
                var imageBytes = await _fileService.ReadFileAsync("ProductImages", targetImageUrl);
                if (imageBytes != null)
                {
                    using var stream = new MemoryStream(imageBytes);
                    using var randomAccessStream = stream.AsRandomAccessStream();
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(randomAccessStream);

                    ImageSource = bitmap;
                    _currentImageUrl = targetImageUrl; // Save state
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResolvedImageUrl)));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating image for product {Id}: {ex.Message}");
            }
        }
    }

    public static class ProductPickerMappingExtensions
    {
        public static async Task<ProductPickerGridItem> ToPickerGridItem(
            this ProductDto dto,
            IFileService fileService)
        {
            var gridItem = new ProductPickerGridItem
            {
                Id = dto.Id,
                Name = dto.Name,
                BaseSku = dto.BaseSku,
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                CategoryPath = dto.CategoryPath,
                BasePriceAmount = dto.BasePriceAmount,
                BasePriceCurrency = dto.BasePriceCurrency,
                MainImageUrl = dto.MainImageUrl,
                HasVariants = dto.HasVariants,
                Options = dto.Options,
                Variants = dto.Variants,
                Images = dto.Images,
                StockOnHand = dto.StockOnHand,
                AvailableQuantity = dto.AvailableQuantity,
                IsActive = dto.IsActive,
                CreatedOnUtc = dto.CreatedOnUtc,
                LastModifiedOnUtc = dto.LastModifiedOnUtc,
            };

            gridItem.InitializeDependencies(fileService);

            if (dto.HasVariants && dto.Options != null)
            {
                foreach (var option in dto.Options)
                {
                    var uiOption = new UiOption(option.Name, option.Values);

                    uiOption.Values.CollectionChanged += (s, e) => { };
                    foreach (var value in uiOption.Values)
                    {
                        value.PropertyChanged += async (s, e) =>
                        {
                            if (e.PropertyName == nameof(UiOptionValue.IsSelected))
                            {
                                gridItem.OnOptionInteraction?.Invoke(gridItem);
                                gridItem.EvaluateResolvedVariant();

                                if (value.IsSelected)
                                {
                                    await gridItem.UpdateImageAsync();
                                }
                            }
                        };
                    }

                    gridItem.UiOptions.Add(uiOption);
                }
            }

            // Evaluate initial state (crucial for products with NO variants)
            gridItem.EvaluateResolvedVariant();

            // Load the initial main image
            await gridItem.UpdateImageAsync();

            return gridItem;
        }
    }
}
