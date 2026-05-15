using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public sealed partial class IssuePoDialog : ContentDialog
    {
        public FulfillmentTaskGridItem Task { get; }

        public bool HasOrderNumber => !string.IsNullOrWhiteSpace(Task.OrderNumber);
        public int MaxAllowedQuantity { get; }
        public string BaseCurrency {  get; }

        // Input Properties mapped back to the ViewModel
        public double IssueQuantity { get; set; }
        public string PoNumber { get; set; } = string.Empty;
        public double CostAmount { get; set; }
        public string EstimatedCostHeader => $"Estimated Cost ({BaseCurrency})*";
        public IssuePoDialog(
            FulfillmentTaskGridItem task,
            int initialQuantity,
            string initialPoNumber = "",
            double initialCost = 0,
            string baseCurrency = "LKR")
        {
            Task = task;

            // Calculate how many items are left to be ordered
            MaxAllowedQuantity = task.RequestedQuantity - task.StartedQuantity;
            BaseCurrency = baseCurrency;

            // Bind the initial draft states
            IssueQuantity = initialQuantity > 0 ? initialQuantity : MaxAllowedQuantity;
            PoNumber = initialPoNumber;
            CostAmount = initialCost;

            InitializeComponent();
            BaseCurrency = baseCurrency;
        }
    }
}
