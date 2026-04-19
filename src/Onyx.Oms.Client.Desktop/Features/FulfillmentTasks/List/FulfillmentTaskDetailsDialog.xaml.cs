using Microsoft.UI.Xaml.Controls;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public sealed partial class FulfillmentTaskDetailsDialog : ContentDialog
    {
        public FulfillmentTaskGridItem Task { get; }

        public bool IsProduction => Task.Type == FulfillmentTaskType.Production;
        public bool IsProcurement => Task.Type == FulfillmentTaskType.Procurement;

        public bool HasOrder => !string.IsNullOrEmpty(Task.OrderNumber);
        public bool HasNotes => !string.IsNullOrEmpty(Task.Notes);
        
        // Formatted helpers
        public string ExpectedDateText => Task.ExpectedCompletionDate.HasValue 
            ? Task.ExpectedCompletionDate.Value.ToString("d") 
            : "Not Set";

        public string CreatedDateText => Task.CreatedOnUtc.ToLocalTime().ToString("g");
        
        public string FormattedCost => Task.Cost != null 
            ? $"{Task.Cost.Amount:N2} {Task.Cost.Currency}" 
            : "N/A";

        public string PoNumberText => string.IsNullOrEmpty(Task.PurchaseOrderNumber)
            ? "N/A" : Task.PurchaseOrderNumber;

        public FulfillmentTaskDetailsDialog(FulfillmentTaskGridItem task)
        {
            Task = task;
            this.InitializeComponent();
        }
    }
}
