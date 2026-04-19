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
        public int TotalRequested { get; set; }
        public int TotalCompleted { get; set; }

        public FulfillmentGroup(Guid productVariantId, string productName, string sku, int totalRequested, int totalCompleted, IEnumerable<FulfillmentTaskGridItem> items) : base(items)
        {
            ProductVariantId = productVariantId;
            ProductName = productName;
            Sku = sku;
            TotalRequested = totalRequested;
            TotalCompleted = totalCompleted;
        }
    }
}
