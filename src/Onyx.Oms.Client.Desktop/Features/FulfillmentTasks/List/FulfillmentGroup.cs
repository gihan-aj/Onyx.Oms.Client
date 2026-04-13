using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public class FulfillmentGroup : ObservableCollection<FulfillmentTaskGridItem>
    {
        public Guid ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalRequested { get; set; }

        public FulfillmentGroup(Guid productVariantId, string productName, int totalRequested, IEnumerable<FulfillmentTaskGridItem> items) : base(items)
        {
            ProductVariantId = productVariantId;
            ProductName = productName;
            TotalRequested = totalRequested;
        }
    }
}
