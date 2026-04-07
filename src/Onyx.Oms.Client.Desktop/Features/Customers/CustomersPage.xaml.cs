using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Threading.Tasks;
using static Onyx.Oms.Client.Desktop.Shared.Constants.Permissions;

namespace Onyx.Oms.Client.Desktop.Features.Customers;

public sealed partial class CustomersPage : Page
{
    public CustomersViewModel ViewModel { get; }

    public CustomersPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<CustomersViewModel>();
        DataContext = ViewModel;

        var permissionService = App.Current.Services.GetRequiredService<IPermissionService>();
        NewCustomerButton.Visibility = permissionService.CanExecute(Shared.Constants.Permissions.Customers.Create)
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        await ViewModel.SearchCommand.ExecuteAsync(args.QueryText);
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        // Reset all columns sorting visually
        if (CustomersDataGrid != null)
        {
            foreach (var col in CustomersDataGrid.Columns)
            {
                col.SortDirection = null;
            }
        }

        await ViewModel.RefreshCommand.ExecuteAsync(null);
    }

    private async void OnDataGridSorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
    {
        var column = e.Column.Tag?.ToString();
        if (string.IsNullOrEmpty(column)) return;

        var direction = e.Column.SortDirection == CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Ascending
            ? "desc"
            : "asc";

        // Reset all columns
        foreach (var col in ((CommunityToolkit.WinUI.UI.Controls.DataGrid)sender).Columns)
        {
            col.SortDirection = null;
        }

        // Set the current column sort direction
        e.Column.SortDirection = direction == "asc"
            ? CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Ascending
            : CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Descending;

        await ViewModel.Sort(column, direction);
    }

    private void OnActionButtonTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        // 1. Stop the DataGrid from stealing this click to select the row
        e.Handled = true;

        // 2. Manually trigger the flyout
        if (sender is Button btn && btn.Flyout != null)
        {
            btn.Flyout.ShowAt(btn);
        }
    }

    private async void OnViewClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is CustomerDto customer)
        {
            var dialog = new CustomerDetailsDialog(customer);
            if(dialog != null)
            {
                dialog.XamlRoot = this.XamlRoot;
                await dialog.ShowAsync();
            }
        }
    }

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is CustomerDto customer)
        {
            ViewModel.EditCustomerCommand.Execute(customer);
        }
    }

    private async void OnActivateClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is CustomerDto customer)
        {
            await ViewModel.ActivateCommand.ExecuteAsync(customer);
        }
    }

    private async void OnDeactivateClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is CustomerDto customer)
        {
            await ViewModel.DeactivateCommand.ExecuteAsync(customer);
        }
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is CustomerDto customer)
        {
            await ViewModel.DeleteCommand.ExecuteAsync(customer);
        }
    }

    private async void OnContextViewClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is CustomerDto selectedCustomer)
        {
            var dialog = new CustomerDetailsDialog(selectedCustomer);
            if (dialog != null)
            {
                dialog.XamlRoot = this.XamlRoot;
                await dialog.ShowAsync();
            }
        }
    }

    private void OnContextEditClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item)
        {
            if (item.DataContext is CustomerDto selectedCustomer)
            {
                ViewModel.EditCustomerCommand.Execute(selectedCustomer);
            }
        }
    }

    private void OnContextActivateClick(object sender, RoutedEventArgs e)
    {
        if(sender is MenuFlyoutItem item)
        {
            if(item.DataContext is CustomerDto selectedCustomer)
            {
                ViewModel.ActivateCommand.Execute(selectedCustomer);
            }
        }
    }

    private void OnContextDeactivateClick(object sender, RoutedEventArgs e)
    {
        if(sender is MenuFlyoutItem item && item.DataContext is CustomerDto selectedCustomer)
        {
            ViewModel.DeactivateCommand.Execute(selectedCustomer);
        }
    }

    private void OnContextDeleteClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is CustomerDto selectedCustomer)
        {
            ViewModel.DeleteCommand.Execute(selectedCustomer);
        }
    }
}
