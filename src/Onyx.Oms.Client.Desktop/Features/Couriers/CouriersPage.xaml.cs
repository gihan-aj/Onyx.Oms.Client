using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public sealed partial class CouriersPage : Page
{
    public CouriersViewModel ViewModel { get; }

    public CouriersPage()
    {
        InitializeComponent();
        
        // Resolve ViewModel from DI (Service Locator)
        ViewModel = App.Current.Services.GetService(typeof(CouriersViewModel)) as CouriersViewModel 
                    ?? throw new InvalidOperationException("CouriersViewModel not found");
        
        DataContext = ViewModel;
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (ViewModel.SearchCommand.CanExecute(args.QueryText))
        {
            ViewModel.SearchCommand.Execute(args.QueryText);
        }
    }

    private void OnDataGridSorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
    {
        if (e.Column.Tag is not string sortColumn) return;

        // Toggle sort direction
        if (e.Column.SortDirection == null || e.Column.SortDirection == CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Descending)
        {
            e.Column.SortDirection = CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Ascending;
        }
        else
        {
            e.Column.SortDirection = CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Descending;
        }

        // Clear sort direction of other columns
        foreach (var column in ((CommunityToolkit.WinUI.UI.Controls.DataGrid)sender).Columns)
        {
            if (column != e.Column)
            {
                column.SortDirection = null;
            }
        }

        // Trigger sort in ViewModel
        var direction = e.Column.SortDirection == CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Ascending ? "asc" : "desc";
        _ = ViewModel.Sort(sortColumn, direction);
    }

    private async void OnNewClick(object sender, RoutedEventArgs e)
    {
        var navigationService = App.Current.Services.GetRequiredService<Shared.Services.INavigationService>();
        navigationService.NavigateTo(typeof(CourierFormViewModel).FullName!);
    }

    private async void OnViewClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not CourierDto courier) return;

        var dialog = new CourierDetailsDialog(courier)
        {
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async void OnEditClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not CourierDto courier) return;

        var navigationService = App.Current.Services.GetRequiredService<Shared.Services.INavigationService>();
        navigationService.NavigateTo(typeof(CourierFormViewModel).FullName!, courier);
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not CourierDto courier) return;

        // We can use IDialogService here since we have access to App.Services, 
        // OR standard ContentDialog. Let's use ContentDialog for consistency or the service if injected.
        // Actually ViewModel has DeleteCommand and DialogService, but for confirmation we usually want UI control.
        // Let's us the DialogService via the ServiceProvider since we didn't inject it into Page (only ViewModel has it).
        // Wait, Page has ViewModel, and ViewModel has DialogService. 
        // But ViewModel's DialogService is for errors. 
        // Let's just use a simple ContentDialog here for confirmation.

        var dialog = new ContentDialog
        {
            Title = "Delete Courier",
            Content = $"Are you sure you want to delete {courier.Name}?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteCourier(courier);
        }
    }

    private async void OnActivateClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not CourierDto courier) return;
        await ViewModel.ActivateCourier(courier);
    }

    private async void OnDeactivateClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not CourierDto courier) return;

        var dialog = new ContentDialog
        {
            Title = "Deactivate Courier",
            Content = $"Are you sure you want to deactivate {courier.Name}?",
            PrimaryButtonText = "Deactivate",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.DeactivateCourier(courier);
        }
    }
}
