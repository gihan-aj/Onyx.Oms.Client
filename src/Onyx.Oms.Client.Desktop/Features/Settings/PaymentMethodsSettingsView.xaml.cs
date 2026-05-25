using CommunityToolkit.WinUI.UI.Controls;
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

namespace Onyx.Oms.Client.Desktop.Features.Settings;

public sealed partial class PaymentMethodsSettingsView : UserControl
{
    public PaymentMethodsViewModel ViewModel
    {
        get => (PaymentMethodsViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(PaymentMethodsViewModel), typeof(PaymentMethodsSettingsView), new PropertyMetadata(null));

    public PaymentMethodsSettingsView()
    {
        InitializeComponent();
    }

    private void PaymentMethodsDataGrid_Sorting(object sender, DataGridColumnEventArgs e)
    {
        if (ViewModel == null) return;
        // Implement your sorting logic here, or map to ViewModel.HandleSort
        if (e.Column.Tag != null)
        {
            //var sortCol = e.Column.Tag.toString();
            // Example: ViewModel.ApplySort(sortCol);
        }
    }

    private async void EditMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;

        var item = (PaymentMethodGridItem)((FrameworkElement)sender).DataContext;

        var dialog = new EditPaymentMethodDialog(item)
        {
            XamlRoot = this.XamlRoot
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.UpdatePaymentMethodAsync(item.Id, dialog.DisplayName, (decimal)dialog.FeeRate);
        }
    }

    private async void ActivateMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;

        var item = (PaymentMethodGridItem)((FrameworkElement)sender).DataContext;
        await ViewModel.ActivatePaymentMethodAsync(item.Id);
    }

    private async void DeactivateMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;

        var item = (PaymentMethodGridItem)((FrameworkElement)sender).DataContext;
        await ViewModel.DeactivatePaymentMethodAsync(item.Id);
    }

    private void OnActionButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        // Prevent the DataGrid row selection event from firing when clicking the meatballs menu
        e.Handled = true;
    }
}
