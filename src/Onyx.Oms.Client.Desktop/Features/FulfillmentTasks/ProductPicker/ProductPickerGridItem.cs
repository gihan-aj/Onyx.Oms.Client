using Microsoft.UI.Xaml.Media.Imaging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Onyx.Oms.Client.Desktop.Shared.Constants.Permissions;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.ProductPicker
{
    public class ProductPickerGridItem : ProductDto
    {
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
        public BitmapImage? ImageSource { get; set; }
        public ObservableCollection<UiOption> UiOptions { get; set; } = new();
        public Action<ProductPickerGridItem>? OnOptionInteraction {  get; set; }

        private ProductVariantDto? _resolvedVariant;
        public ProductVariantDto? ResolvedVariant
        {
            get => _resolvedVariant;
            private set
            {
                if(_resolvedVariant != value)
                {
                    _resolvedVariant = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResolvedVariant)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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

            if(selectedDict.Count == Options.Count)
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

            // Image Logic
            if (!string.IsNullOrWhiteSpace(dto.MainImageUrl))
            {
                try
                {
                    var imageBytes = await fileService.ReadFileAsync("ProductImages", dto.MainImageUrl);
                    if(imageBytes != null)
                    {
                        using var stream = new MemoryStream(imageBytes);
                        using var randomAccessStream = stream.AsRandomAccessStream();
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(randomAccessStream);
                        gridItem.ImageSource = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading image for product {dto.Id}: {ex.Message}");
                }
            }

            if(dto.HasVariants && dto.Options != null)
            {
                foreach(var option in dto.Options)
                {
                    var uiOption = new UiOption(option.Name, option.Values);

                    uiOption.Values.CollectionChanged += (s, e) => { };
                    foreach (var value in uiOption.Values)
                    {
                        value.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(UiOptionValue.IsSelected))
                            {
                                gridItem.OnOptionInteraction?.Invoke(gridItem);
                                gridItem.EvaluateResolvedVariant();
                            }
                        };
                    }

                    gridItem.UiOptions.Add(uiOption);
                }
            }

            // Evaluate initial state (crucial for products with NO variants)
            gridItem.EvaluateResolvedVariant();

            return gridItem;
        }
    }
}
