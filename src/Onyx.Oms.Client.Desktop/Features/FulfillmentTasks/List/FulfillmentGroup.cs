using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public class FulfillmentGroup : ObservableCollection<FulfillmentTaskGridItem>
    {
        public Guid ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public IEnumerable<VariantAttributeDto>? VariantAttributes { get; set; }
        public int TotalRequested { get; set; }
        public int TotalCompleted { get; set; }
        public IAsyncRelayCommand<FulfillmentGroup> CompleteBatchCommand { get; }

        public FulfillmentGroup(
            Guid productVariantId, 
            string productName, 
            string sku, 
            IEnumerable<VariantAttributeDto>? variantAttributes, 
            int totalRequested, 
            int totalCompleted,
            IAsyncRelayCommand<FulfillmentGroup> completeBatchCommand,
            IEnumerable<FulfillmentTaskGridItem> items) : base(items)
        {
            ProductVariantId = productVariantId;
            ProductName = productName;
            Sku = sku;
            VariantAttributes = variantAttributes;
            TotalRequested = totalRequested;
            TotalCompleted = totalCompleted;
            CompleteBatchCommand = completeBatchCommand;
        }
    }
}
