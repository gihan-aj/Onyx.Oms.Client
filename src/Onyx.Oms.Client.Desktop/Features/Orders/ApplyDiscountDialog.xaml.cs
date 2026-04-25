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
using Onyx.Oms.Client.Desktop.Shared.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.Orders;

public sealed partial class ApplyDiscountDialog : ContentDialog
{
    public OrderDiscountDto? Result { get; private set; }

    public double DiscountValue { get; set; }
    public string Reason { get; set; } = string.Empty;

    public List<DiscountType> DiscountTypes { get; } = new() 
    { 
        DiscountType.Percentage, 
        DiscountType.FlatAmount 
    };

    public DiscountType SelectedType { get; set; } = DiscountType.Percentage;

    public ApplyDiscountDialog()
    {
        InitializeComponent();
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (DiscountValue <= 0)
        {
            args.Cancel = true;
            return;
        }

        Result = new OrderDiscountDto(
            Value: (decimal)DiscountValue,
            Type: SelectedType,
            Reason: string.IsNullOrWhiteSpace(Reason) ? null : Reason
        );
    }
}
