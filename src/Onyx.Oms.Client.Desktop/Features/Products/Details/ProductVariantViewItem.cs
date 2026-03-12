using System.Linq;

namespace Onyx.Oms.Client.Desktop.Features.Products.Details
{
    public class ProductVariantViewItem
    {
        public ProductDetailsVariantDto Dto { get; }
        public string Description => string.Join(" / ", Dto.Attributes.Select(a => a.Value));
        public int AvailableQuantity => Dto.StockOnHand - Dto.ReservedQuantity;
        public ProductVariantViewItem(ProductDetailsVariantDto dto) 
        {
            Dto = dto;
        }
    }
}
