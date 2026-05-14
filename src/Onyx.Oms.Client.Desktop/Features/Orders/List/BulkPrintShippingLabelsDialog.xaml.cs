using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.Orders.List;

public sealed partial class BulkPrintShippingLabelsDialog : ContentDialog
{
    private readonly IOrdersApi _ordersApi;
    public ObservableCollection<OrderGridItem> Orders { get; } = new();
    public ObservableCollection<OrderGridItem> FilteredOrders { get; } = new();

    public List<string> StatusFilters { get; } = new() { "All", "Ready To Pack", "Packed" };
    public List<Guid> SelectedOrderIds => Orders.Select(o => o.Id).ToList();

    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
        nameof(IsLoading), typeof(bool), typeof(BulkPrintShippingLabelsDialog), new PropertyMetadata(false));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    private string _selectedStatusFilter = "All";
    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            _selectedStatusFilter = value;
            ApplyFilter();
        }
    }
    public string PrimaryButtonLabel => $"Generate Labels ({Orders.Count})";
    public BulkPrintShippingLabelsDialog(IOrdersApi ordersApi)
    {
        _ordersApi = ordersApi;
        InitializeComponent();
    }

    private async void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        await LoadEligibleOrdersAsync();
    }
    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadEligibleOrdersAsync();
    }

    private async Task LoadEligibleOrdersAsync()
    {
        IsLoading = true;
        Orders.Clear();
        FilteredOrders.Clear();
        try
        {
            // Fetch top 200 orders that are ReadyToPack or Packed
            var result = await _ordersApi.GetOrdersPaged(
                page: 1,
                pageSize: 200,
                searchTerm: null,
                sortColumn: "OrderDate",
                sortOrder: "DESC",
                statuses: new[] { OrderStatus.ReadyToPack, OrderStatus.Packed },
                paymentStatus: null,
                customerId: null,
                courierId: null,
                isCashOnDelivery: null,
                fromDate: null,
                toDate: null);
            foreach (var item in result.Items)
            {
                // Convert DTO to GridItem (assuming CanEdit/CanView defaults to true for the dialog)
                Orders.Add(item.ToGridItem(true, true));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load orders: {ex.Message}");
        }
        finally
        {
            ApplyFilter();
            IsLoading = false;
        }

    }

    private void ApplyFilter()
    {
        FilteredOrders.Clear();
        foreach (var order in Orders)
        {
            if (SelectedStatusFilter == "All" ||
               (SelectedStatusFilter == "Ready To Pack" && order.Status == OrderStatus.ReadyToPack) ||
               (SelectedStatusFilter == "Packed" && order.Status == OrderStatus.Packed))
            {
                FilteredOrders.Add(order);
            }
        }

        PrimaryButtonText = $"Generate Labels ({FilteredOrders.Count})";
    }

    private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid orderId)
        {
            var order = Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                Orders.Remove(order);
                ApplyFilter(); // Reapply filter to remove from the view
            }
        }
    }
}
