using Microsoft.UI.Xaml.Controls;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public sealed partial class CourierDetailsDialog : ContentDialog
{
    public CourierDto Courier { get; }

    public CourierDetailsDialog(CourierDto courier)
    {
        InitializeComponent();
        Courier = courier;
    }
}
