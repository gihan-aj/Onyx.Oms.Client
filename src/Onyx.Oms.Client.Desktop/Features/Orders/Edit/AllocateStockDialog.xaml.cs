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

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit;

public sealed partial class AllocateStockDialog : ContentDialog
{
    public string ProductName { get; }
    public string Sku { get; }
    public int PendingQuantity { get; }
    public int AvailableQuantity { get; }

    // Limit allocation to either what they need or what is actually available
    public int MaxAllocatable => Math.Min(PendingQuantity, AvailableQuantity);
    public int QuantityToAllocate { get; set; }

    public AllocateStockDialog(string productName, string sku, int pendingQuantity, int availableQuantity)
    {
        ProductName = productName;
        Sku = sku;
        PendingQuantity = pendingQuantity;
        AvailableQuantity = availableQuantity;

        // Default the input to allocating everything possible
        QuantityToAllocate = MaxAllocatable;
        InitializeComponent();
    }
}
