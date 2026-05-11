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

namespace Onyx.Oms.Client.Desktop.Features.Orders;

public sealed partial class CustomerOrderHistoryDialog : ContentDialog
{
    public int TotalOrdersCount { get; set; }
    public List<CustomerOrderHistoryItem> RecentOrders { get; private set; }
    public CustomerOrderHistoryDialog(CustomerOrderHistoryResponse orderHistory)
    {
        InitializeComponent();
        TotalOrdersCount = orderHistory.TotalOrdersCount;
        RecentOrders = orderHistory.RecentOrders?
            .Select(x => new CustomerOrderHistoryItem(x))
            .ToList() ?? new List<CustomerOrderHistoryItem>();

    }
}

public class CustomerOrderHistoryItem
{
    private readonly CustomerOrderSummaryDto _dto;
    public CustomerOrderHistoryItem(CustomerOrderSummaryDto dto)
    {
        _dto = dto;
    }
    public string OrderNumber => _dto.OrderNumber;

    // Format to local time
    public string OrderDateDisplay => _dto.OrderDate.HasValue
        ? _dto.OrderDate.Value.ToLocalTime().ToString("MMM dd, yyyy h:mm tt")
        : "-";
    public OrderStatus Status => _dto.Status;
    public PaymentStatus PaymentStatus => _dto.PaymentStatus;

    public decimal GrandTotalAmount => _dto.GrandTotalAmount;
    public string GrandTotalCurrency => _dto.GrandTotalCurrency;

    public decimal BalanceAmount => _dto.BalanceAmount;
    public bool HasDueBalance => BalanceAmount > 0;
}
