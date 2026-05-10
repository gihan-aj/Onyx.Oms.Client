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

namespace Onyx.Oms.Client.Desktop.Features.Orders;

public sealed partial class ShippingConfirmationDialog : ContentDialog
{
    public string? CourierName { get; }
    public string? ShippingAddress { get; }
    public string? TrackingNumber { get; set; }
    // Logic Flags for the UI
    public bool HasCourier => !string.IsNullOrWhiteSpace(CourierName);
    public bool HasAddress => !string.IsNullOrWhiteSpace(ShippingAddress);
    public bool HasMissingInfo => !HasCourier || !HasAddress;

    // Only allow clicking "Mark as Shipped" if both are present
    public bool CanShip => HasCourier && HasAddress;
    public ShippingConfirmationDialog(string? courierName, string? shippingAddress)
    {
        InitializeComponent();
        CourierName = courierName;
        ShippingAddress = shippingAddress;
    }
}
