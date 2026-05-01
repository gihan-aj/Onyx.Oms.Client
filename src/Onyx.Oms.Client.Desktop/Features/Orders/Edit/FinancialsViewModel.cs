using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class FinancialsViewModel : ObservableObject
    {
        private OrderStatus _orderStatus;
        private List<OrderItemDto> _orderItems;
        private decimal _originalShippingCost;
        private decimal _originaltaxAmount;
        private decimal _originalDiscountAmount;
        private string? _originalDiscountReason;

        private bool _canEdit;
        public bool CanEdit
        {
            get => _canEdit;
            set => SetProperty(ref _canEdit, value);
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
                    CanEdit = _orderStatus < OrderStatus.Shipped && !value;
                }
            }
        }

        public bool IsReadonly => !IsEditing;

        private string _baseCurrency = "LKR";
        public string BaseCurrency
        {
            get => _baseCurrency;
            set => SetProperty(ref _baseCurrency, value);
        }

        private decimal _shippingFee;
        public decimal ShippingFee
        {
            get => _shippingFee;
            set
            {
                if (SetProperty(ref _shippingFee, value))
                {
                    //RecalculateTotals();
                }
            }
        }

        private decimal _taxAmount;
        public decimal TaxAmount
        {
            get => _taxAmount;
            set
            {
                if (SetProperty(ref _taxAmount, value))
                {
                    //RecalculateTotals();
                }
            }
        }

        private OrderDiscountDto? _appliedDiscount;
        public OrderDiscountDto? AppliedDiscount
        {
            get => _appliedDiscount;
            set
            {
                if (SetProperty(ref _appliedDiscount, value))
                {
                    OnPropertyChanged(nameof(HasDiscount));
                    OnPropertyChanged(nameof(ShowOriginalDiscount));
                    OnPropertyChanged(nameof(ShowNewDiscount));
                    OnPropertyChanged(nameof(ShowApplyButton));
                    //RecalculateTotals();
                }
            }
        }

        public bool HasDiscount => AppliedDiscount != null;

        public decimal OriginalDiscountAmount => _originalDiscountAmount;
        public string? OriginalDiscountReason => _originalDiscountReason;

        public bool HasOriginalDiscount => _originalDiscountAmount > 0;
        public bool ShowOriginalDiscount => HasOriginalDiscount && AppliedDiscount == null;
        public bool ShowNewDiscount => AppliedDiscount != null;
        public bool ShowApplyButton => !HasOriginalDiscount && AppliedDiscount == null;

        public IRelayCommand BeginEditCommand { get; }
        public IRelayCommand CancelEditCommand { get; }
        public IAsyncRelayCommand ShowApplyDiscountDialogCommand { get; }
        public IRelayCommand ClearDiscountCommand { get; }

        public FinancialsViewModel(OrderDetailsDto order)
        {
            _orderStatus = order.Status;
            _orderItems = order.Items
                .Select(i => new OrderItemDto(i.Id, i.ProductVariantId, i.Quantity, null))
                .ToList();
            _originalShippingCost = order.ShippingCost;
            _originaltaxAmount = order.TaxAmount;
            _originalDiscountAmount = order.DiscountAmount;
            _originalDiscountReason = order.DiscountReason;

            BaseCurrency = order.BaseCurrency;
            ShippingFee = order.ShippingCost;
            TaxAmount = order.TaxAmount;

            CanEdit = _orderStatus < OrderStatus.Shipped;

            BeginEditCommand = new RelayCommand(BeginEdit);
            CancelEditCommand = new RelayCommand(CancelEdit);
            ShowApplyDiscountDialogCommand = new AsyncRelayCommand(ShowApplyDiscountDialogAsync);
            ClearDiscountCommand = new RelayCommand(() => AppliedDiscount = null);
        }

        private void BeginEdit()
        {
            RestoreOriginalValues();
            IsEditing = true;
        }

        private void CancelEdit()
        {
            RestoreOriginalValues();
            IsEditing = false;
        }

        void RestoreOriginalValues()
        {
            ShippingFee = _originalShippingCost;
            TaxAmount = _originaltaxAmount;
            AppliedDiscount = null;
        }

        private async Task ShowApplyDiscountDialogAsync()
        {
            var dialog = new ApplyDiscountDialog();
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;

            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && dialog.Result != null)
            {
                AppliedDiscount = dialog.Result;
            }
        }

        public UpdateOrderFinancialsCommand GetUpdateDto()
        {
            var shippingCost = new MoneyDto(ShippingFee, BaseCurrency);
            var tax = new MoneyDto(TaxAmount, BaseCurrency);
            return new UpdateOrderFinancialsCommand(_orderItems, shippingCost, tax, AppliedDiscount);
        }
    }
}
