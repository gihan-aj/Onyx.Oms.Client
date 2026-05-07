using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit;

public sealed partial class CreateFulfillmentTaskDialog : ContentDialog
{
    public string OrderNumber { get; }
    public string ProductName { get; }
    public string Sku { get; }
    public int PendingQuantity { get; }
    public FulfillmentTaskType TaskType { get; }
    public int RequestedQuantity { get; set; }
    public DateTimeOffset? ExpectedCompletionDate { get; set; }
    public string? Notes { get; set; }

    public List<TaskPriority> Priorities { get; } = new()
    {
        TaskPriority.Low,
        TaskPriority.Normal,
        TaskPriority.High,
        TaskPriority.Urgent
    };
    public TaskPriority SelectedPriority { get; set; } = TaskPriority.Normal;
    public CreateFulfillmentTaskDialog(string orderNumber, string productName, string sku, int pendingQuantity, FulfillmentTaskType taskType)
    {
        InitializeComponent();
        OrderNumber = orderNumber;
        ProductName = productName;
        Sku = sku;
        PendingQuantity = pendingQuantity;
        TaskType = taskType;
        RequestedQuantity = pendingQuantity;
        Title = taskType == FulfillmentTaskType.Production ? "Create Production Task" : "Create Procurement Task";
    }
}
