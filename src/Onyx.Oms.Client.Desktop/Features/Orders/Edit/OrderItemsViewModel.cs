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
            _baseCurrency = order.BaseCurrency;
            _shippingCost = order.ShippingCost;
            _taxAmount = order.TaxAmount;
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
                    BaseCurrency = order.BaseCurrency,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    AvailableQuantity = item.AvailableQuantity,
                    AllocatedQuantity = item.AllocatedQuantity,
                    PendingQuantity = item.PendingQuantity,
                    RemoveCommand = RemoveLineItemCommand
                };

                Items.Add(orderItem);
            }

            RecalculateSubTotal();

            CanModifyCart = _orderStatus < OrderStatus.Shipped;

            BeginEditCommand = new AsyncRelayCommand(BeginEdit);
            CancelEditCommand = new AsyncRelayCommand(CancelEdit);
            ShowProductPickerCommand = new AsyncRelayCommand(ShowProductPickerAsync);
            RemoveLineItemCommand = new RelayCommand<EditOrderLineItem>(RemoveLineItem);
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
                    BaseCurrency = _baseCurrency,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    AvailableQuantity = item.AvailableQuantity,
                    AllocatedQuantity = item.AllocatedQuantity,
                    PendingQuantity = item.PendingQuantity,
                    RemoveCommand = RemoveLineItemCommand
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
                    BaseCurrency = gridItem.BasePriceCurrency,
                    UnitPrice = price,
                    Quantity = qty,
                    AvailableQuantity = gridItem.ResolvedVariant != null
                        ? (gridItem.ResolvedVariant.StockOnHand - gridItem.ResolvedVariant.ReservedQuantity)
                        : gridItem.AvailableQuantity,
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
