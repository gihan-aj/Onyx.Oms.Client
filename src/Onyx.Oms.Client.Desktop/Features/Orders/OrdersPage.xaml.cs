using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Features.Orders;

public sealed partial class OrdersPage : Page
{
    public OrdersViewModel ViewModel { get; set; }

    public OrdersPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<OrdersViewModel>();
        DataContext = ViewModel;

        CustomerPicker.FetchDataDelegate = ViewModel.FetchCustomersAsync;
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.LoadDataCommand.Execute(null);
    }

    private async void OrdersDataGrid_Sorting(object sender, DataGridColumnEventArgs e)
    {
        var tag = e.Column.Tag?.ToString();
        if (string.IsNullOrEmpty(tag))
            return;

        foreach (var column in OrdersDataGrid.Columns.OfType<DataGridBoundColumn>())
        {
            if (column != e.Column)
                column.SortDirection = null;
        }

        var sortOrder = e.Column.SortDirection == DataGridSortDirection.Ascending ? "asc" : "desc";
        await ViewModel.SortByAsync(tag, sortOrder);
    }

    private void OnActionButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is Button btn && btn.Flyout != null)
        {
            btn.Flyout.ShowAt(btn);
        }
    }

    private void ViewMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: OrderGridItem order })
            ViewModel.ViewDetailsCommand.Execute(order);
        else if (sender is MenuFlyoutItem item && item.DataContext is OrderGridItem selectedOrder)
            ViewModel.ViewDetailsCommand.Execute(selectedOrder);
    }

    private void EditMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: OrderGridItem order })
            ViewModel.EditDetailsCommand.Execute(order);
        else if (sender is MenuFlyoutItem item && item.DataContext is OrderGridItem selectedOrder)
            ViewModel.EditDetailsCommand.Execute(selectedOrder);
    }
}
