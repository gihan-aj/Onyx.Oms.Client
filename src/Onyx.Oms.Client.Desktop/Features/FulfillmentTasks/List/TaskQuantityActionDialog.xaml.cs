using Microsoft.UI.Xaml.Controls;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public sealed partial class TaskQuantityActionDialog : ContentDialog
    {
        public FulfillmentTaskGridItem Task { get; }

        public string ActionTitle { get; }
        public string ActionMessage { get; }
        public bool HasOrderNumber => !string.IsNullOrWhiteSpace(Task.OrderNumber);

        public int MaxAllowedQuantity { get; }
        public double InputValue { get; set; }

        public TaskQuantityActionDialog(
            FulfillmentTaskGridItem task,
            string actionTitle,
            string actionMessage,
            int maxAllowedQuantity,
            int? initialValue = null)
        {
            Task = task;
            ActionTitle = actionTitle;
            ActionMessage = actionMessage;
            MaxAllowedQuantity = maxAllowedQuantity;
            if (initialValue != null)
                InputValue = initialValue.Value;
            else
                InputValue = maxAllowedQuantity;

            this.InitializeComponent();
        }
    }
}
