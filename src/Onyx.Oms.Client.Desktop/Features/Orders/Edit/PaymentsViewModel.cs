using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class PaymentsViewModel : ObservableObject
    {
        private readonly Guid _orderId;
        private readonly OrderStatus _status;
        private readonly bool _isCashOnDelivery;
        private readonly IToastService _toastService;
        private readonly IOrdersApi _ordersApi;
        private readonly ILogger<PaymentsViewModel> _logger;

        public ObservableCollection<OrderPaymentLineItem> Payments { get; } = new();

        private decimal _totalPaid;
        public decimal TotalPaid { get => _totalPaid; private set => SetProperty(ref _totalPaid, value); }

        private decimal _dueBalance;
        public decimal DueBalance
        {
            get => _dueBalance;
            private set
            {
                if (SetProperty(ref _dueBalance, value))
                    OnPropertyChanged(nameof(CanAddPayment));
            }
        }
        public string BaseCurrency { get; }
        // Only allow adding payments if there is a balance owed (or if balance is negative and we need to refund)
        public bool CanAddPayment
        {
            get
            {
                if (_isCashOnDelivery)
                {
                    return DueBalance != 0 && _status == OrderStatus.Delivered;
                }
                return DueBalance != 0;
            }
        }

        public bool ShowPayments
        {
            get
            {
                if (_isCashOnDelivery)
                {
                    return _status >= OrderStatus.Delivered;
                }

                return true;
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _hasPayments = false;
        public bool HasPayments
        {
            get => _hasPayments;
            set => SetProperty(ref _hasPayments, value);
        }

        public IAsyncRelayCommand ShowAddPaymentDialogCommand { get; }
        public PaymentsViewModel(OrderDetailsDto order, IToastService toastService, IOrdersApi ordersApi)
        {
            _toastService = toastService;
            _ordersApi = ordersApi;
            _logger = App.Current.Services.GetRequiredService<ILogger<PaymentsViewModel>>();

            _orderId = order.Id;
            _status = order.Status;
            _isCashOnDelivery = order.IsCashOnDelivery;
            BaseCurrency = order.BaseCurrency;

            TotalPaid = order.TotalPaid;
            DueBalance = order.BalanceAmount;
            foreach (var p in order.Payments)
            {
                Payments.Add(new OrderPaymentLineItem(p, BaseCurrency));
            }
            ShowAddPaymentDialogCommand = new AsyncRelayCommand(ShowAddPaymentDialogAsync);

            if(Payments.Any())
                HasPayments = true;
        }

        private async Task ShowAddPaymentDialogAsync()
        {
            var dialog = new AddPaymentDialog(DueBalance, BaseCurrency, _isCashOnDelivery)
            {
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                var command = new AddPaymentCommand(
                    dialog.PaymentAmount,
                    BaseCurrency,
                    dialog.SelectedMethod,
                    dialog.ReferenceNumber,
                    dialog.PaymentDate.Date + dialog.PaymentTime);

                try
                {
                    IsBusy = true;
                    var paymentId = await _ordersApi.AddPayment(_orderId, command);
                    var newPayment = new OrderPaymentDetailsDto(paymentId, dialog.PaymentAmount,dialog.SelectedMethod, dialog.ReferenceNumber, dialog.PaymentDate.Date + dialog.PaymentTime, null, null, null);
                    Payments.Add(new OrderPaymentLineItem(newPayment, BaseCurrency));

                    if (!HasPayments)
                        HasPayments = true;

                    TotalPaid += dialog.PaymentAmount;
                    DueBalance -= dialog.PaymentAmount;
                    _toastService.ShowSuccess("Payment Added", "Transaction recorded successfully.");
                }
                catch
                {
                    _logger.LogError("Failed to update order logistics details for order ID: {OrderId}", _orderId);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}
