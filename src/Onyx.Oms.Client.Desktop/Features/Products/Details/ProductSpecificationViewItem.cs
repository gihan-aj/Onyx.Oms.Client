namespace Onyx.Oms.Client.Desktop.Features.Products.Details
{
    public class ProductSpecificationViewItem
    {
        public string Label { get; }
        public string Value { get; }
        public ProductSpecificationViewItem(ProductSpecificationDto spec) 
        {
            Label = spec.Label;
            Value = spec.Value;
        }
    }
}
