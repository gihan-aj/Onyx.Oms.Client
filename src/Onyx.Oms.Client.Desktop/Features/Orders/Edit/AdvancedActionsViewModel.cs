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

        public AdvancedActionsViewModel(
            IOrdersApi ordersApi,
            IToastService toastService)
        {
            _ordersApi = ordersApi;
            _toastService = toastService;
            _logger = App.Current.Services.GetRequiredService<ILogger<AdvancedActionsViewModel>>();

            RevertShipmentCommand = new AsyncRelayCommand(RevertShipmentAsync);
        }

        public void Configure(Guid orderId, OrderStatus status, string? rollbackReason)
        {
            _orderId = orderId;
            CanRevertShipment = status == OrderStatus.Shipped;
            // Future actions:
            // CanRevertPacking = status == OrderStatus.Packed;
            RollbackReason = rollbackReason;

            ShouldShowAdvacedActions = status == OrderStatus.Shipped || !string.IsNullOrEmpty(rollbackReason);
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
    }
}
