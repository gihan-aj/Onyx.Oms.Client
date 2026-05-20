using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
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

namespace Onyx.Oms.Client.Desktop.Features.Orders.List
{
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

        public Visibility IsAllActiveTab(OrderCategoryTab tab) => tab == OrderCategoryTab.All ? Visibility.Visible : Visibility.Collapsed;

        private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem is SelectorBarItem selectedItem && selectedItem.Tag is string tag)
            {
                if (Enum.TryParse<OrderCategoryTab>(tag, out var tab))
                {
                    ViewModel.SelectedTab = tab;
                }
            }
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

        private OrderGridItem? GetOrderFromSender(object sender)
        {
            if (sender is FrameworkElement { DataContext: OrderGridItem order }) return order;
            if (sender is MenuFlyoutItem item && item.DataContext is OrderGridItem selectedOrder) return selectedOrder;
            return null;
        }

        private void ManageMenuItem_Click(object sender, RoutedEventArgs e) => ViewModel.ManageOrderCommand.Execute(GetOrderFromSender(sender));
        private async void DownloadInvoiceMenuItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.DataContext is OrderGridItem order)
            {
                await ViewModel.DownloadInvoiceAsync(order);
            }
        }
        private async void DownloadShippingLabelMenuItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.DataContext is OrderGridItem order)
            {
                await ViewModel.DownloadShippingLabelAsync(order);
            }
        }
        private void ConfirmMenuItem_Click(object sender, RoutedEventArgs e) => ViewModel.ConfirmOrderCommand.Execute(GetOrderFromSender(sender));
        private void CancelMenuItem_Click(object sender, RoutedEventArgs e) => ViewModel.CancelOrderCommand.Execute(GetOrderFromSender(sender));
        private void PackMenuItem_Click(object sender, RoutedEventArgs e) => ViewModel.PackOrderCommand.Execute(GetOrderFromSender(sender));
        private void ShipMenuItem_Click(object sender, RoutedEventArgs e) => ViewModel.ShipOrderCommand.Execute(GetOrderFromSender(sender));
        private void DeliverMenuItem_Click(object sender, RoutedEventArgs e) => ViewModel.DeliverOrderCommand.Execute(GetOrderFromSender(sender));
        private void CompleteMenuItem_Click(object sender, RoutedEventArgs e) => ViewModel.CompleteOrderCommand.Execute(GetOrderFromSender(sender));
        private void FailDeliveryMenuItem_Click(object sender, RoutedEventArgs e) => ViewModel.FailDeliveryCommand.Execute(GetOrderFromSender(sender));
        private void ReceiveReturnMenuItem_Click(object sender, RoutedEventArgs e) => ViewModel.ReceiveReturnCommand.Execute(GetOrderFromSender(sender));
        private void ProcessReturnMenuItem_Click(object sender, RoutedEventArgs e) => ViewModel.ProcessReturnCommand.Execute(GetOrderFromSender(sender));
    }
}
