using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List;

public sealed partial class CompleteBatchDialog : ContentDialog
{
    public string ProductName { get; }
    public string Sku { get; }
    public int RemainingQuantity { get; }
    public bool AllocateToOrders { get; set; } = true;

    public CompleteBatchDialog(string productName, string sku, int remainingQuantity)
    {
        InitializeComponent();
        ProductName = productName;
        Sku = sku;
        RemainingQuantity = remainingQuantity;
    }
}
