using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class OrderItemsViewModel : ObservableObject
    {
        private readonly IFileService _fileService;
        private readonly IToastService _toastService;

        private OrderStatus _orderStatus;
        private readonly string _orderNumber;

        private bool _canModifyCart;
        public bool CanModifyCart
        {
            get => _canModifyCart;
            set => SetProperty(ref _canModifyCart, value);
        }

        private bool _isEditing = false;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    OnPropertyChanged(nameof(IsReadonly));
                    //OnPropertyChanged(nameof(CanModifyCart));
                    CanModifyCart = !value && _orderStatus < OrderStatus.Shipped;
                }
            }
        }

        public bool IsReadonly => !IsEditing;

        private string _baseCurrency = string.Empty;
        private decimal _shippingCost;
        private decimal _taxAmount;

        private List<OrderItemDetailsDto> _originalItems = new();

        public ObservableCollection<EditOrderLineItem> Items = new();

        private decimal _subTotal;
        public decimal SubTotal
        {
            get => _subTotal;
            private set => SetProperty(ref _subTotal, value);
        }

        public Func<Guid, Guid, int, Task>? OnAllocateStockRequested { get; set; }
        public IAsyncRelayCommand<EditOrderLineItem> AllocateStockCommand { get; }
        public Func<Guid, Guid, FulfillmentTaskType, int, TaskPriority, DateTimeOffset?, string?, Task>? OnCreateTaskRequested { get; set; }
        public IAsyncRelayCommand<EditOrderLineItem> CreateProductionTaskCommand { get; }
        public IAsyncRelayCommand<EditOrderLineItem> CreateProcurementTaskCommand { get; }
        public IAsyncRelayCommand BeginEditCommand { get; }
        public IAsyncRelayCommand CancelEditCommand { get; }
        public IAsyncRelayCommand ShowProductPickerCommand { get; }
        public IRelayCommand<EditOrderLineItem> RemoveLineItemCommand { get; }

        public OrderItemsViewModel(OrderDetailsDto order, IFileService fileService, IToastService toastService)
        {
            _fileService = fileService;
            _toastService = toastService;

            _originalItems = order.Items;

            _orderStatus = order.Status;
            _orderNumber = order.OrderNumber;
            _baseCurrency = order.BaseCurrency;
            _shippingCost = order.ShippingCost;
            _taxAmount = order.TaxAmount;

            CreateProductionTaskCommand = new AsyncRelayCommand<EditOrderLineItem>(i => CreateTaskAsync(i, FulfillmentTaskType.Production));
            CreateProcurementTaskCommand = new AsyncRelayCommand<EditOrderLineItem>(i => CreateTaskAsync(i, FulfillmentTaskType.Procurement));
            AllocateStockCommand = new AsyncRelayCommand<EditOrderLineItem>(AllocateStockAsync);
            BeginEditCommand = new AsyncRelayCommand(BeginEdit);
            CancelEditCommand = new AsyncRelayCommand(CancelEdit);
            ShowProductPickerCommand = new AsyncRelayCommand(ShowProductPickerAsync);
            RemoveLineItemCommand = new RelayCommand<EditOrderLineItem>(RemoveLineItem);

            foreach (var item in order.Items)
            {
                var orderItem = new EditOrderLineItem(_fileService)
                {
                    Id = item.Id,
                    OrderCurrentStatus = order.Status,
                    ProductId = item.Id,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.ProductName,
                    Sku = item.Sku,
                    ImageUrl = item.ImageUrl,
                    Status = item.Status,
                    WeightUnit = item.WeightUnit,
                    UnitWeight = item.UnitWeight,
                    BaseCurrency = order.BaseCurrency,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    AvailableQuantity = item.AvailableQuantity,
                    AllocatedQuantity = item.AllocatedQuantity,
                    PendingQuantity = item.PendingQuantity,
                    IncomingStock = item.IncomingStock,
                    AllocateStockCommand = AllocateStockCommand,
                    CreateProcurementTaskCommand = CreateProcurementTaskCommand,
                    CreateProductionTaskCommand = CreateProductionTaskCommand,
                    RemoveCommand = RemoveLineItemCommand,
                    ShowFulfillmentDetails = _orderStatus < OrderStatus.Shipped
                };

                Items.Add(orderItem);
            }

            RecalculateSubTotal();

            CanModifyCart = _orderStatus < OrderStatus.Shipped;
            
        }

        private async Task BeginEdit()
        {
            //await RestoreOriginalValues();
            IsEditing = true;
        }

        private async Task CancelEdit()
        {
            await RestoreOriginalValues();
            IsEditing = false;
        }

        private async Task RestoreOriginalValues()
        {
            Items.Clear();

            foreach (var item in _originalItems)
            {
                var orderItem = new EditOrderLineItem(_fileService)
                {
                    Id = item.Id,
                    OrderCurrentStatus = _orderStatus,
                    ProductId = item.Id,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.ProductName,
                    Sku = item.Sku,
                    ImageUrl = item.ImageUrl,
                    Status = item.Status,
                    WeightUnit = item.WeightUnit,
                    UnitWeight = item.UnitWeight,
                    BaseCurrency = _baseCurrency,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    AvailableQuantity = item.AvailableQuantity,
                    AllocatedQuantity = item.AllocatedQuantity,
                    PendingQuantity = item.PendingQuantity,
                    IncomingStock = item.IncomingStock,
                    AllocateStockCommand = AllocateStockCommand,
                    CreateProcurementTaskCommand = CreateProcurementTaskCommand,
                    CreateProductionTaskCommand = CreateProductionTaskCommand,
                    RemoveCommand = RemoveLineItemCommand,
                    ShowFulfillmentDetails = _orderStatus < OrderStatus.Shipped
                };

                Items.Add(orderItem);
            }

            await LoadImagesAsync();
            RecalculateSubTotal();
        }

        public async Task LoadImagesAsync()
        {
            foreach(var item in Items)
                await item.LoadImageAsync();
        }

        private async Task ShowProductPickerAsync()
        {
            var dialog = new ProductPicker.ProductPicker();
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;

            dialog.ViewModel.OnProductAdded = async (gridItem, qty) =>
            {
                decimal price = gridItem.ResolvedVariant != null && gridItem.ResolvedVariant.PriceAmount > 0
                    ? gridItem.ResolvedVariant.PriceAmount
                    : gridItem.BasePriceAmount;

                var lineItem = new EditOrderLineItem(_fileService)
                {
                    Id = null,
                    OrderCurrentStatus = _orderStatus,
                    ProductId = gridItem.Id,
                    ProductVariantId = gridItem.ResolvedVariant?.Id ?? Guid.Empty,
                    ProductName = gridItem.DisplayName,
                    Sku = gridItem.DisplaySku,
                    ImageUrl = gridItem.ResolvedImageUrl,
                    WeightUnit = gridItem.ResolvedVariant?.WeightUnit ?? "kg",
                    UnitWeight = gridItem.ResolvedVariant?.WeightAmount ?? 0,
                    BaseCurrency = gridItem.BasePriceCurrency,
                    UnitPrice = price,
                    Quantity = qty,
                    AvailableQuantity = gridItem.ResolvedVariant != null
                        ? (gridItem.ResolvedVariant.StockOnHand - gridItem.ResolvedVariant.ReservedQuantity)
                        : gridItem.AvailableQuantity,
                    IncomingStock = gridItem.IncomingStock,
                    Status = OrderItemStatus.Pending,
                    RemoveCommand = RemoveLineItemCommand
                };

                var existingItem = Items.FirstOrDefault(i => i.ProductId == lineItem.ProductId && i.ProductVariantId == lineItem.ProductVariantId);
                if (existingItem != null)
                {
                    existingItem.Quantity += lineItem.Quantity;
                }
                else
                {
                    await lineItem.LoadImageAsync();
                    Items.Add(lineItem);
                }

                RecalculateSubTotal();
                _toastService.ShowSuccess("Item Added", $"Added {qty}x {lineItem.ProductName} to the order.");
            };

            await dialog.ShowAsync();
        }

        private void RemoveLineItem(EditOrderLineItem? item)
        {
            if (item != null && Items.Contains(item))
            {
                Items.Remove(item);
                RecalculateSubTotal();
            }
        }

        public void RecalculateSubTotal()
        {
            SubTotal = Items.Sum(i => i.LineTotal);
        }

        public decimal CalculateTotalWeight()
        {
            return Items.Sum(i => i.UnitWeight * i.Quantity);
        }

        private async Task AllocateStockAsync(EditOrderLineItem? item)
        {
            if (item == null) return;
            var dialog = new AllocateStockDialog(item.ProductName, item.Sku, item.PendingQuantity, item.AvailableQuantity)
            {
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            var result = await dialog.ShowAsync();

            // If they clicked 'Allocate' and the parent is listening, send the data up!
            if (result == ContentDialogResult.Primary && OnAllocateStockRequested != null)
            {
                if(item.Id.HasValue)
                    await OnAllocateStockRequested(item.Id.Value, item.ProductVariantId, dialog.QuantityToAllocate);
                else
                    _toastService.ShowError("Unsaved changes", "Seems like you have unsaved changes in order items.");
            }
        }

        private async Task CreateTaskAsync(EditOrderLineItem? item, FulfillmentTaskType taskType)
        {
            // Important: We need the OrderItemId (item.Id), so we can't create tasks for unsaved items!
            if (item == null || item.Id == null)
            {
                _toastService.ShowError("Save Order First", "You must save the order items before creating tasks.");
                return;
            }
            var dialog = new CreateFulfillmentTaskDialog(_orderNumber, item.ProductName, item.Sku, item.PendingQuantity, taskType)
            {
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && OnCreateTaskRequested != null)
            {
                await OnCreateTaskRequested(item.Id.Value, item.ProductVariantId, taskType, dialog.RequestedQuantity, dialog.SelectedPriority, dialog.ExpectedCompletionDate, dialog.Notes);
            }
        }

        public UpdateOrderFinancialsCommand? GetUpdateDto()
        {
            if(Items.Count == 0)
            {
                _toastService.ShowError("Validation Error", "Please add at least one item to the order.");
                return null;
            }
            var orderItems = Items.Select(i => new OrderItemDto(i.Id, i.ProductVariantId, i.Quantity, null)).ToList();
            var shippingCost = new MoneyDto(_shippingCost, _baseCurrency);
            var tax = new MoneyDto(_taxAmount, _baseCurrency);
            var dto = new UpdateOrderFinancialsCommand(orderItems, shippingCost, tax, null);
            return dto;
        }
    }
}
