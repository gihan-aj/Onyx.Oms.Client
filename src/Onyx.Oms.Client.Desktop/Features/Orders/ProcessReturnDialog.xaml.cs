using CommunityToolkit.Mvvm.ComponentModel;
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.Orders;

public sealed partial class ProcessReturnDialog : ContentDialog
{
    private readonly IOrdersApi _ordersApi;
    private readonly Guid _orderId;
    public ObservableCollection<ReturnItemViewModel> ItemsToReturn { get; } = new();
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
        }
    }
    public string Reason { get; set; } = string.Empty;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ProcessReturnDialog(IOrdersApi ordersApi, Guid orderId)
    {
        InitializeComponent();
        _ordersApi = ordersApi;
        _orderId = orderId;
    }

    private async void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        IsLoading = true;
        try
        {
            var orderDetails = await _ordersApi.GetOrderById(_orderId);
            ItemsToReturn.Clear();
            foreach (var item in orderDetails.Items)
            {
                ItemsToReturn.Add(new ReturnItemViewModel
                {
                    OrderItemId = item.Id,
                    ProductName = item.ProductName,
                    Sku = item.Sku,
                    MaxQuantity = item.Quantity, // Ordered quantity
                    ReturnQuantity = item.Quantity // Default to returning all
                });
            }
        }
        catch
        {
            // You can add error logging or a toast message here if you want
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class ReturnItemViewModel : ObservableObject
{
    public Guid OrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int MaxQuantity { get; set; }

    private int _returnQuantity;
    public int ReturnQuantity
    {
        get => _returnQuantity;
        set => SetProperty(ref _returnQuantity, value);
    }
}
