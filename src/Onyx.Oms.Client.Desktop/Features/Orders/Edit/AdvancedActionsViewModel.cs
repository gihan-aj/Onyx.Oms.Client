using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class AdvancedActionsViewModel : ObservableObject
    {
        private readonly IOrdersApi _ordersApi;
        private readonly IToastService _toastService;
        private readonly ILogger<AdvancedActionsViewModel> _logger;
        private Guid? _orderId;

        private bool _shouldShowAdvacedActions;
        public bool ShouldShowAdvacedActions
        {
            get => _shouldShowAdvacedActions;
            set => SetProperty(ref _shouldShowAdvacedActions, value);
        }

        private bool _canRevertShipment;
        public bool CanRevertShipment
        {
            get => _canRevertShipment;
            set => SetProperty(ref _canRevertShipment, value);
        }

        private bool _canRevertPacking;
        public bool CanRevertPacking
        {
            get => _canRevertPacking;
            set => SetProperty(ref _canRevertPacking, value);
        }

        private bool _canRevertToPending;
        public bool CanRevertToPending
        {
            get => _canRevertToPending;
            set => SetProperty(ref _canRevertToPending, value);
        }

        private string? _rollbackReason;
        public string? RollbackReason
        {
            get => _rollbackReason;
            set
            {
                if (SetProperty(ref _rollbackReason, value))
                {
                    OnPropertyChanged(nameof(HasRollbackHistory));
                }
            }
        }

        public bool HasRollbackHistory => !string.IsNullOrWhiteSpace(RollbackReason);

        public Func<Task>? OnActionCompleted { get; set; }

        public IAsyncRelayCommand RevertShipmentCommand { get; }
        public IAsyncRelayCommand UnpackCommand { get; }
        public IAsyncRelayCommand RevertToPendingCommand { get; }

        public AdvancedActionsViewModel(
            IOrdersApi ordersApi,
            IToastService toastService)
        {
            _ordersApi = ordersApi;
            _toastService = toastService;
            _logger = App.Current.Services.GetRequiredService<ILogger<AdvancedActionsViewModel>>();

            RevertShipmentCommand = new AsyncRelayCommand(RevertShipmentAsync);
            UnpackCommand = new AsyncRelayCommand(UnpackAsync);
            RevertToPendingCommand = new AsyncRelayCommand(RevertToPendingAsync);
        }

        public void Configure(Guid orderId, OrderStatus status, string? rollbackReason)
        {
            _orderId = orderId;
            CanRevertShipment = status == OrderStatus.Shipped;
            CanRevertPacking = status == OrderStatus.Packed;
            CanRevertToPending = status == OrderStatus.Confirmed || status == OrderStatus.Processing || status == OrderStatus.ReadyToPack;
            RollbackReason = rollbackReason;

            ShouldShowAdvacedActions = CanRevertShipment || CanRevertPacking || CanRevertToPending || !string.IsNullOrEmpty(rollbackReason);
        }

        private async Task RevertShipmentAsync()
        {
            if (!_orderId.HasValue) return;

            var reasonBox = new TextBox
            {
                PlaceholderText = "Required — enter a reason for the audit log",
                AcceptsReturn = false,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                MinHeight = 80
            };

            var dialog = new ContentDialog
            {
                Title = "Revert Shipment?",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            Text        = "This will cancel the shipment, void the tracking number, and return the items to 'Packed' inventory status. Please provide a reason for the audit log.",
                            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                        },
                        reasonBox
                    }
                },
                PrimaryButtonText = "Revert Shipment",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;
            string reason = reasonBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(reason))
            {
                _toastService.ShowError("Reason Required", "You must provide a reason to revert the shipment.");
                return;
            }

            try
            {
                var command = new RollbackOrderToPackedRequest(reason);
                await _ordersApi.RollbackOrderToPacked(_orderId.Value, command);
                _toastService.ShowSuccess("Shipment Reverted", "The order has been returned to 'Packed' status.");
                if (OnActionCompleted != null)
                    await OnActionCompleted.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revert shipment for order {OrderId}", _orderId);
                //_toastService.ShowError("Failed", "Could not revert the shipment. Please try again.");
            }
        }
        private async Task UnpackAsync()
        {
            if (!_orderId.HasValue) return;

            var reasonBox = new TextBox
            {
                PlaceholderText = "Required — enter a reason for the audit log",
                AcceptsReturn = false,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                MinHeight = 80
            };

            var dialog = new ContentDialog
            {
                Title = "Unpack?",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            Text        = "This will return the items to 'Ready To Pack' inventory status. Please provide a reason for the audit log.",
                            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                        },
                        reasonBox
                    }
                },
                PrimaryButtonText = "Unpack",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;
            string reason = reasonBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(reason))
            {
                _toastService.ShowError("Reason Required", "You must provide a reason to unpack the items.");
                return;
            }

            try
            {
                var command = new UnpackRequest(reason);
                await _ordersApi.Unpack(_orderId.Value, command);
                _toastService.ShowSuccess("Items Unpacked!", "The order has been returned to 'Ready To Pack' status.");
                if (OnActionCompleted != null)
                    await OnActionCompleted.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unpack items for order {OrderId}", _orderId);
                //_toastService.ShowError("Failed", "Could not revert the shipment. Please try again.");
            }
        }
        private async Task RevertToPendingAsync()
        {
            if (!_orderId.HasValue) return;

            var reasonBox = new TextBox
            {
                PlaceholderText = "Required — enter a reason for the audit log",
                AcceptsReturn = false,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                MinHeight = 80
            };

            var dialog = new ContentDialog
            {
                Title = "Revert To Pending?",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            Text        = "This will release reservation, unlink any fulfillment tasks from order items, and return the items to 'Pending' inventory status. Please provide a reason for the audit log.",
                            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                        },
                        reasonBox
                    }
                },
                PrimaryButtonText = "Revert to Pending",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;
            string reason = reasonBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(reason))
            {
                _toastService.ShowError("Reason Required", "You must provide a reason to revert the shipment.");
                return;
            }

            try
            {
                var command = new RollbackOrderToPendingRequest(reason);
                await _ordersApi.RollbackOrderToPending(_orderId.Value, command);
                _toastService.ShowSuccess("Order Unconfirmed", "The order has been returned to 'Pending' status.");
                if (OnActionCompleted != null)
                    await OnActionCompleted.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revert order to pending for order {OrderId}", _orderId);
                //_toastService.ShowError("Failed", "Could not revert the shipment. Please try again.");
            }
        }
    }
}
