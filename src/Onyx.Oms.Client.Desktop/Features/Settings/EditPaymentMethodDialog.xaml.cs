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

namespace Onyx.Oms.Client.Desktop.Features.Settings;

public sealed partial class EditPaymentMethodDialog : ContentDialog
{
    public string DisplayName { get; set; }
    public double FeeRate { get; set; }
    public EditPaymentMethodDialog(PaymentMethodGridItem item)
    {
        InitializeComponent();

        // Initialize with the current item's values
        DisplayName = item.DisplayName;
        FeeRate = (double)item.FeeRate;
    }
}
