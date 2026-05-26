using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public sealed partial class CourierFormPage : Page
{
    public CourierFormViewModel ViewModel { get; }

    public CourierFormPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<CourierFormViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (ViewModel is Shared.Services.INavigationAware navAware)
        {
            navAware.OnNavigatedTo(e.Parameter);
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (ViewModel is Shared.Services.INavigationAware navAware)
        {
            navAware.OnNavigatedFrom();
        }
    }

    private async void OnAddZoneRateClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var newItem = new ZoneRateFormItem
        {
            BaseWeight = 1,
            Currency   = "LKR",
            WeightUnit = "kg"
        };

        var dialog = new ZoneRateDialog(newItem, isEdit: false)
        {
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            ViewModel.ZoneRates.Add(dialog.Item);
            ViewModel.EnsureSingleDefault(dialog.Item);
        }
    }

    private async void OnEditZoneRateClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if ((sender as Microsoft.UI.Xaml.Controls.Button)?.Tag is not ZoneRateFormItem original)
            return;

        // Clone so the dialog can be cancelled without mutating the live item
        var clone = original.Clone();

        var dialog = new ZoneRateDialog(clone, isEdit: true)
        {
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            original.CopyFrom(dialog.Item);
            ViewModel.EnsureSingleDefault(original);
        }
    }
    private void OnRemoveZoneRateClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if ((sender as Microsoft.UI.Xaml.Controls.Button)?.Tag is ZoneRateFormItem item)
            ViewModel.RemoveZoneRateCommand.Execute(item);
    }
}
