using Microsoft.UI.Xaml.Media.Imaging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products.List
{
    public class ProductGridItem : ProductDto
    {
        public string HasVariantsText => HasVariants ? "Yes" : "No";
        public bool CanEdit { get; set; }
        public bool CanActivate { get; set; }
        public bool CanDeactivate { get; set; }
        public BitmapImage? MainImageSource { get; set; }
        public bool IsOutOfStock => AvailableQuantity <= 0;
        public bool IsLowStock => AvailableQuantity > 0 && AvailableQuantity <= 10;
        public bool IsInStock => AvailableQuantity > 10; 
    }

    public static class ProductMappingExtensions
    {
        public static async Task<ProductGridItem> ToGridItem(
            this ProductDto dto,
            bool canEdit,
            bool canActivate,
            bool canDeactivate,
            IFileService fileService)
        {
            var gridItem = new ProductGridItem
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
                StockOnHand = dto.StockOnHand,
                AvailableQuantity = dto.AvailableQuantity,
                IsActive = dto.IsActive,
                CreatedOnUtc = dto.CreatedOnUtc,
                LastModifiedOnUtc = dto.LastModifiedOnUtc,

                // Map UI/Permission properties
                CanEdit = canEdit,
                CanActivate = canActivate && !dto.IsActive, // Example logic: can only activate if inactive
                CanDeactivate = canDeactivate && dto.IsActive,
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
                        gridItem.MainImageSource = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading image for product {dto.Id}: {ex.Message}");
                }
            }

            return gridItem;
        }
    }
}
