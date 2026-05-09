using Microsoft.UI.Xaml.Controls;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public sealed partial class TaskQuantityActionDialog : ContentDialog
    {
        public FulfillmentTaskGridItem Task { get; }

        public string ActionTitle { get; }
        public string ActionMessage { get; }
        public bool ShowAllocateToOrderCheck { get; }
        public bool HasOrderNumber => !string.IsNullOrWhiteSpace(Task.OrderNumber);
        public bool? AllocateToOrder { get; set; }

        public int MaxAllowedQuantity { get; }
        public double InputValue { get; set; }

        public TaskQuantityActionDialog(
            FulfillmentTaskGridItem task,
            string actionTitle,
            string actionMessage,
            int maxAllowedQuantity,
            bool isCompleteAction,
            int? initialValue = null)
        {
            this.InitializeComponent();

            Task = task;
            ActionTitle = actionTitle;
            ActionMessage = actionMessage;
            MaxAllowedQuantity = maxAllowedQuantity;
            if (initialValue != null)
                InputValue = initialValue.Value;
            else
                InputValue = maxAllowedQuantity;

            if (HasOrderNumber)
                AllocateToOrder = true;

            ShowAllocateToOrderCheck = isCompleteAction && HasOrderNumber;
        }
    }
}
