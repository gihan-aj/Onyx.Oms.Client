using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Features.Couriers;
using Onyx.Oms.Client.Desktop.Features.Orders.List;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class EditOrderViewModel : ObservableObject, INavigationAware
    {
        private readonly IOrdersApi _ordersApi;
        private readonly ILogger<EditOrderViewModel> _logger;
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastService;
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;

        private Guid? _orderId;
        private Guid? _customerId;
        private bool _isCod;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _isCalculatingShipping;
        public bool IsCalculatingShipping
        {
            get => _isCalculatingShipping;
            set => SetProperty(ref _isCalculatingShipping, value);
        }

        private string _pageTitle = string.Empty;
        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        private OrderStatus _status = OrderStatus.Pending;
        public OrderStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        // Customer
        private CustomerDetailsViewModel? _customerDetails = null;
        public CustomerDetailsViewModel? CustomerDetails
        {
            get => _customerDetails;
            set
            {
                if (SetProperty(ref _customerDetails, value))
                {
                    OnPropertyChanged(nameof(HasCustomerDetails));
                }
            }
        }

        // Logistics
        private OrderLogisticsViewModel? _logistics = null;
        public OrderLogisticsViewModel? Logistics
        {
            get => _logistics;
            set => SetProperty(ref _logistics, value);
        }

        public bool HasCustomerDetails => CustomerDetails != null;

        // Order items
        private OrderItemsViewModel? _orderItems = null;
        public OrderItemsViewModel? OrderItems
        {
            get => _orderItems;
            set => SetProperty(ref _orderItems, value);
        }

        // Financials
        private FinancialsViewModel? _financials = null;
        public FinancialsViewModel? Financials
        {
            get => _financials;
            set => SetProperty(ref _financials, value);
        }

        // Financial Summary
        private string _baseCurrency = "R";
        public string BaseCurrency
        {
            get => _baseCurrency;
            set => SetProperty(ref _baseCurrency, value);
        }

        private decimal _discountAmount;
        public decimal DiscountAmount
        {
            get => _discountAmount;
            private set => SetProperty(ref _discountAmount, value);
        }
        private decimal _grandTotal;
        public decimal GrandTotal
        {
            get => _grandTotal;
            private set => SetProperty(ref _grandTotal, value);
        }

        // Payments
        private PaymentsViewModel? _payments;
        public PaymentsViewModel? Payments 
        { 
            get => _payments; 
            set => SetProperty(ref _payments, value); 
        }

        // Notes
        private NotesViewModel? _notes;
        public NotesViewModel? Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        // Advanced Actions (Danger Zone)
        private AdvancedActionsViewModel? _advancedActions;
        public AdvancedActionsViewModel? AdvancedActions
        {
            get => _advancedActions;
            set => SetProperty(ref _advancedActions, value);
        }


        // Status
        private bool _canConfirm = false;
        public bool CanConfirm
        {
            get => _canConfirm;
            set => SetProperty(ref _canConfirm, value);
        }

        private bool _canPack = false;
        public bool CanPack
        {
            get => _canPack;
            set => SetProperty(ref _canPack, value);
        }

        private bool _canShip = false;
        public bool CanShip
        {
            get => _canShip;
            set => SetProperty(ref _canShip, value);
        }

        private bool _canDeliver = false;
        public bool CanDeliver
        {
            get => _canDeliver;
            set => SetProperty(ref _canDeliver, value);
        }

        private bool _canComplete = false;
        public bool CanComplete
        {
            get => _canComplete;
            set => SetProperty(ref _canComplete, value);
        }

        private bool _canDownloadInvoice = false;
        public bool CanDownloadInvoice
        {
            get => _canDownloadInvoice;
            set => SetProperty(ref _canDownloadInvoice, value);
        }

        private string _downloadButtonText = "Invoice";
        public string DownloadButtonText
        {
            get => _downloadButtonText;
            set => SetProperty(ref _downloadButtonText, value);
        }

        public IRelayCommand GoBackCommand { get; }
        public IAsyncRelayCommand UpdateOrderLogisticsCommand { get; }
        public IAsyncRelayCommand UpdateOrderItemsCommand { get; }
        public IAsyncRelayCommand UpdateFinancialsCommand { get; }
        public IAsyncRelayCommand UpdateNotesCommand { get; }
        public IAsyncRelayCommand ConfirmOrderCommand { get; }
        public IAsyncRelayCommand PackOrderCommand { get; }
        public IAsyncRelayCommand ShipOrderCommand { get; }
        public IAsyncRelayCommand DeliverOrderCommand { get; }
        public IAsyncRelayCommand CompleteOrderCommand { get; }
        public IAsyncRelayCommand ShowCustomerOrderHistoryCommand { get; }
        public IAsyncRelayCommand DownloadInvoiceCommand { get; }
        public IAsyncRelayCommand CalculateShippingFeeCommand { get; }

        public EditOrderViewModel(IOrdersApi ordersApi, ILogger<EditOrderViewModel> logger, INavigationService navigationService, IToastService toastService, IFileService fileService, IDialogService dialogService)
        {
            _ordersApi = ordersApi;
            _logger = logger;
            _navigationService = navigationService;
            _toastService = toastService;
            _fileService = fileService;
            _dialogService = dialogService;

            GoBackCommand = new RelayCommand(GoBack);
            UpdateOrderLogisticsCommand = new AsyncRelayCommand(UpdateLogisticsAsync);
            UpdateOrderItemsCommand = new AsyncRelayCommand(UpdateOrderItemsAsync);
            UpdateFinancialsCommand = new AsyncRelayCommand(UpdateFinancialsAsync);
            UpdateNotesCommand = new AsyncRelayCommand(UpdateNotesAsync);
            ConfirmOrderCommand = new AsyncRelayCommand(ConfirmOrderAsync);
            PackOrderCommand = new AsyncRelayCommand(PackOrderAsync);
            ShipOrderCommand = new AsyncRelayCommand(ShipOrderAsync);
            DeliverOrderCommand = new AsyncRelayCommand(DeliverOrderAsync);
            CompleteOrderCommand = new AsyncRelayCommand(CompleteOrderAsync);
            ShowCustomerOrderHistoryCommand = new AsyncRelayCommand(ShowCustomerOrderHistoryAsync);
            DownloadInvoiceCommand = new AsyncRelayCommand(DownloadInvoiceAsync);
            CalculateShippingFeeCommand = new AsyncRelayCommand(AutoCalculateShippingAsync);
        }

        public void OnNavigatedFrom()
        {
            
        }

        public async void OnNavigatedTo(object parameter)
        {
            if(parameter is Guid orderId)
            {
               await InitializeAsync(orderId);
            }
            else if (parameter is string orderIdString && Guid.TryParse(orderIdString, out var parsedId))
            {
                await InitializeAsync(parsedId);
            }
        }

        private async Task InitializeAsync(Guid orderId)
        {
            try
            {
                IsLoading = true;
                var orderDetails = await _ordersApi.GetOrderById(orderId);

                // Unsubscribe fromprevious viewmodels
                if(OrderItems != null)
                {
                    OrderItems.PropertyChanged -= OnOrderItemsPropertyChanged;
                    //OrderItems.OnAllocateStockRequested -= OnAllocateStockRequested;
                    //OrderItems.OnCreateTaskRequested -= OnCreateTaskRequested;
                }
                if (Financials != null)
                    Financials.PropertyChanged -= OnFinancialsPropertyChanged;
                if(Payments != null)
                    Payments.PropertyChanged -= OnOrderPaymentsPropertyChanged;

                _orderId = orderDetails.Id;
                _isCod = orderDetails.IsCashOnDelivery;
                _customerId = orderDetails.Customer.Id;
                PageTitle = orderDetails.OrderNumber;
                Status = orderDetails.Status;
                CustomerDetails = new CustomerDetailsViewModel(orderDetails.Customer);
                Logistics = new OrderLogisticsViewModel(orderDetails, _toastService);
                Logistics.OnCourierSelected = AutoCalculateShippingAsync;
                await LoadCouriersAsync(Logistics);
                OrderItems = new OrderItemsViewModel(orderDetails, _fileService, _toastService);
                await OrderItems.LoadImagesAsync();
                Financials = new FinancialsViewModel(orderDetails);
                BaseCurrency = orderDetails.BaseCurrency;
                Payments = new PaymentsViewModel(orderDetails, _toastService, _ordersApi);
                Notes = new NotesViewModel(orderDetails);

                AdvancedActions = new AdvancedActionsViewModel(_ordersApi, _toastService);
                AdvancedActions.OnActionCompleted = () => InitializeAsync(_orderId!.Value);
                AdvancedActions.Configure(orderDetails.Id, orderDetails.Status, orderDetails.RollbackReason);

                CanConfirm = orderDetails.Status == OrderStatus.Pending;
                CanPack = orderDetails.Status == OrderStatus.ReadyToPack;
                CanShip = orderDetails.Status == OrderStatus.Packed;
                CanDeliver = orderDetails.Status == OrderStatus.Shipped;
                CanComplete = orderDetails.Status == OrderStatus.Delivered;
                CanDownloadInvoice = orderDetails.Status != OrderStatus.Pending 
                    || orderDetails.Status != OrderStatus.Cancelled 
                    || orderDetails.Status != OrderStatus.DeliveryFailed
                    || orderDetails.Status != OrderStatus.ReturnedToSender;
                if (orderDetails.PaymentStatus == PaymentStatus.FullyPaid)
                    DownloadButtonText = "Receipt";

                RecalculateTotals();

                // Listen to SubTotal changes
                OrderItems.PropertyChanged += OnOrderItemsPropertyChanged;
                OrderItems.OnAllocateStockRequested = OnAllocateStockRequested;
                OrderItems.OnCreateTaskRequested = OnCreateTaskRequested;
                // Listen to Financial changes
                Financials.PropertyChanged += OnFinancialsPropertyChanged;
                Payments.PropertyChanged += OnOrderPaymentsPropertyChanged;
            }
            catch
            {
                _logger.LogError("Failed to load order details for order ID: {OrderId}", orderId);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnOrderItemsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderItemsViewModel.SubTotal))
                RecalculateTotals();
        }
        private void OnFinancialsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FinancialsViewModel.ShippingFee) ||
                e.PropertyName == nameof(FinancialsViewModel.TaxAmount) ||
                e.PropertyName == nameof(FinancialsViewModel.AppliedDiscount))
            {
                RecalculateTotals();
            }
        }
        private void OnOrderPaymentsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PaymentsViewModel.IsBusy) && Payments != null)
                IsBusy = Payments.IsBusy;
        }

        private async Task OnAllocateStockRequested(Guid orderItemId, Guid variantId, int quantityToAllcate)
        {
            if (_orderId.HasValue)
            {
                try
                {
                    IsBusy = true;
                    var command = new AllocateOrderItemQuantityCommand(quantityToAllcate);
                    await _ordersApi.AllocateOrderItemQuantity(_orderId.Value, orderItemId, command);

                    _toastService.ShowSuccess("Stock Allocated", $"Successfully allocated {quantityToAllcate} items to the order.");

                    await InitializeAsync(_orderId.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to allocate quantity to order item at order edit page.");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task OnCreateTaskRequested(Guid orderItemId, Guid variantId, FulfillmentTaskType taskType, int qty, TaskPriority priority, DateTimeOffset? date, string? notes)
        {
            if (_orderId.HasValue)
            {
                try
                {
                    IsBusy = true;
                    if (taskType == FulfillmentTaskType.Production)
                    {
                        var command = new CreateOrderProductionTaskCommand(orderItemId, qty, notes, date, priority);
                        await _ordersApi.CreateProductionTask(_orderId.Value, orderItemId, command);
                    }
                    else
                    {
                        var command = new CreateOrderProcurementTaskCommand(orderItemId, qty, notes, date, priority);
                        await _ordersApi.CreateProcurementTask(_orderId.Value, orderItemId, command);
                    }

                    _toastService.ShowSuccess("Task Created", "Fulfillment task pushed successfully.");

                    // Refresh the page to get the updated incoming stock & statuses!
                    await InitializeAsync(_orderId.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create task at order edit page.");
                }
                finally
                {
                    IsBusy = false;
                }
            }
            
        }

        public string FormatCurrency(decimal value)
        {
            return value.ToString("N2");
        }

        public void RecalculateTotals()
        {
            if(Financials != null && OrderItems != null)
            {
                DiscountAmount = Financials.GetEffectiveDiscountAmount(OrderItems.SubTotal);
                GrandTotal = OrderItems.SubTotal + Financials.ShippingFee + Financials.TaxAmount - DiscountAmount;
            }
        }

        private async Task LoadCouriersAsync(OrderLogisticsViewModel logistics)
        {
            try
            {
                var couriers = await _ordersApi.GetCouriers(true);
                logistics.Couriers.Clear();
                foreach (var c in couriers)
                {
                    logistics.Couriers.Add(c);
                }
                if (logistics.CourierId.HasValue)
                {
                    var selected = logistics.Couriers.FirstOrDefault(c => c.Id == logistics.CourierId.Value);
                    if(selected != null)
                        logistics.SelectedCourier = selected;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load couriers at order edit page.");
            }
        }

        private async Task ShowCustomerOrderHistoryAsync()
        {
            if (!_customerId.HasValue)
                return;

            try
            {
                IsBusy = true;
                var orderHistory = await _ordersApi.GetCustomerOrderHistory(_customerId.Value);
                var dialog = new CustomerOrderHistoryDialog(orderHistory)
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                IsBusy = false;

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to show customer order history at order edit page.");
            }
        }

        private async Task AutoCalculateShippingAsync()
        {
            if (Logistics == null || Logistics.SelectedCourier == null || string.IsNullOrWhiteSpace(Logistics.ShippingAddressDistrict))
            {
                //_toastService.ShowError("Missing Shipping Details", "The Courier and the Shipping Address are needed to calculate the Shipping Fee");
                return;
            }

            if (Financials == null)
                return;

            if (OrderItems == null)
                return;

            if (Payments == null)
                return;

            try
            {
                IsCalculatingShipping = true;

                decimal codAmount = 0;
                if (_isCod)
                {
                    codAmount = OrderItems.SubTotal + Financials.TaxAmount - Financials.OriginalDiscountAmount - Payments.TotalPaid;
                }

                decimal totalWeight = OrderItems.CalculateTotalWeight();

                decimal calculatedFee = await _ordersApi.CalculateShippingFee(
                    Logistics.SelectedCourier.Id,
                    Logistics.ShippingAddressDistrict,
                    totalWeight,
                    codAmount);

                Financials.ShippingFee = calculatedFee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate shipping fee at order edit page.");   
            }
            finally
            {
                IsCalculatingShipping = false;
            }
        }

        private void GoBack()
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
            else
            {
                _navigationService.NavigateTo("Onyx.Oms.Client.Desktop.Features.Orders.List.OrdersPage");
            }
        }

        private async Task UpdateLogisticsAsync()
        {
            if (Logistics == null || !_orderId.HasValue)
                return;

            var request = Logistics.GetUpdateDto();
            if (request == null)
                return;

            try
            {
                IsBusy = true;
                await _ordersApi.UpdateLogistics(_orderId.Value, request);
                await InitializeAsync(_orderId.Value);
                _toastService.ShowSuccess("Success", "Order logistics has been updated.");
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

        private async Task UpdateOrderItemsAsync()
        {
            if (OrderItems == null || !_orderId.HasValue)
                return;

            var request = OrderItems.GetUpdateDto();
            if (request == null)
                return;

            try
            {
                IsBusy = true;
                await _ordersApi.UpdateFinancials(_orderId.Value, request);
                await InitializeAsync(_orderId.Value);
                _toastService.ShowSuccess("Success", "Order items has been updated.");
            }
            catch
            {
                _logger.LogError("Failed to update order items for order ID: {OrderId}", _orderId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateFinancialsAsync()
        {
            if (Financials == null || !_orderId.HasValue)
                return;

            var request = Financials.GetUpdateDto();
            if (request == null)
                return;

            try
            {
                IsBusy = true;
                await _ordersApi.UpdateFinancials(_orderId.Value, request);
                await InitializeAsync(_orderId.Value);
                _toastService.ShowSuccess("Success", "Order financials has been updated.");
            }
            catch
            {
                _logger.LogError("Failed to update order financials for order ID: {OrderId}", _orderId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateNotesAsync()
        {
            if (Notes == null || !_orderId.HasValue)
                return;

            var request = Notes.GetUpdateDto();
            if (request == null)
                return;

            try
            {
                IsBusy = true;
                await _ordersApi.UpdateNotes(_orderId.Value, request);
                await InitializeAsync(_orderId.Value);
                _toastService.ShowSuccess("Success", "Order notes has been updated.");
            }
            catch
            {
                _logger.LogError("Failed to update order notes for order ID: {OrderId}", _orderId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ConfirmOrderAsync()
        {
            if (OrderItems == null || !_orderId.HasValue)
                return;

            bool hasStockShortage = OrderItems.Items.Any(item => item.Quantity > item.AvailableQuantity);

            string title;
            string message;

            if (hasStockShortage)
            {
                title = "Confirm Order (Stock Shortage)";
                message = "You do not have enough stock to fulfill all items in this order immediately. " +
                          "Once confirmed, you will need to create Procurement or Production tasks to acquire the missing items.\n\n" +
                          "Are you sure you want to confirm this order?";
            }
            else
            {
                title = "Confirm Order";
                message = "All items in this order are currently in stock! " +
                          "Once confirmed, this order can be sent for immediate packing.\n\n" +
                          "Are you sure you want to proceed?";
            }

            bool isConfirmed = await _dialogService.ShowConfirmationAsync(title, message, "Confirm Order", "Cancel");
            if (!isConfirmed)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await _ordersApi.ConfirmOrder(_orderId.Value);
                await InitializeAsync(_orderId.Value);
                _toastService.ShowSuccess("Success", "Order has been confirmed.");
            }
            catch
            {
                _logger.LogError("Failed to confirm order for order ID: {OrderId}", _orderId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task PackOrderAsync()
        {
            if (OrderItems == null || !_orderId.HasValue)
                return;

            bool hasStockShortage = OrderItems.Items.Any(item => item.PendingQuantity > 0);
            if (hasStockShortage)
            {
                _toastService.ShowError("Missing Items", "Order cannot be packed until all items are reserved.");
                return;
            }

            string title = "Pack Order";
            string message = "Are you sure you want to mark this order as Packed?";

            bool isConfirmed = await _dialogService.ShowConfirmationAsync(title, message, "Mark as Packed", "Cancel");
            if (!isConfirmed)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await _ordersApi.PackOrder(_orderId.Value);
                await InitializeAsync(_orderId.Value);
                _toastService.ShowSuccess("Success", "Order has been marked as Packed.");
            }
            catch
            {
                _logger.LogError("Failed to mark as Packed order ID: {OrderId}", _orderId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShipOrderAsync()
        {
            if (OrderItems == null || !_orderId.HasValue || Logistics == null)
                return;

            bool hasStockShortage = OrderItems.Items.Any(item => item.PendingQuantity > 0);
            if (hasStockShortage)
            {
                _toastService.ShowError("Missing Items", "Order cannot be shipped until all items are reserved.");
                return;
            }

            string? courierName = Logistics.SelectedCourier?.Name;

            string? address = null;
            if (!string.IsNullOrWhiteSpace(Logistics.ShippingAddressStreet))
            {
                address = $"{Logistics.ShippingAddressStreet}, {Logistics.ShippingAddressCity}, {Logistics.ShippingAddressDistrict}";
            }

            var dialog = new ShippingConfirmationDialog(courierName, Logistics.TrackingNumber, address)
            {
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            var result = await dialog.ShowAsync();

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && Logistics.CourierId.HasValue)
            {
                var command = new ShipOrderCommand(Logistics.CourierId.Value, dialog.TrackingNumber);
                try
                {
                    IsBusy = true;
                    await _ordersApi.ShipOrder(_orderId.Value, command);
                    await InitializeAsync(_orderId.Value);
                    _toastService.ShowSuccess("Success", "Order has been marked as Shipped.");
                }
                catch
                {
                    _logger.LogError("Failed to mark as Shipped order ID: {OrderId}", _orderId);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task DeliverOrderAsync()
        {
            if (OrderItems == null || !_orderId.HasValue || Logistics == null)
                return;

            bool hasStockShortage = OrderItems.Items.Any(item => item.PendingQuantity > 0);
            if (hasStockShortage)
            {
                _toastService.ShowError("Missing Items", "Order cannot be delivered until all items are reserved.");
                return;
            }

            if (!Logistics.CourierId.HasValue)
            {
                _toastService.ShowError("Courier Required", "Order cannot be delivered without a courier.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Logistics.ShippingAddressStreet))
            {
                _toastService.ShowError("Shipping Address Required", "Order cannot be delivered without a shipping address.");
                return;
            }

            string title = "Deliver Order";
            string message = "Are you sure you want to mark this order as Delivered?";

            bool isConfirmed = await _dialogService.ShowConfirmationAsync(title, message, "Mark as Delivered", "Cancel");
            if (!isConfirmed)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await _ordersApi.DeliverOrder(_orderId.Value);
                await InitializeAsync(_orderId.Value);
                _toastService.ShowSuccess("Success", "Order has been marked as Delivered.");
            }
            catch
            {
                _logger.LogError("Failed to mark as Delivered order ID: {OrderId}", _orderId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CompleteOrderAsync()
        {
            if (OrderItems == null || !_orderId.HasValue || Logistics == null || Payments == null)
                return;

            bool hasStockShortage = OrderItems.Items.Any(item => item.PendingQuantity > 0);
            if (hasStockShortage)
            {
                _toastService.ShowError("Missing Items", "Order cannot be delivered until all items are reserved.");
                return;
            }

            if (!Logistics.CourierId.HasValue)
            {
                _toastService.ShowError("Courier Required", "Order cannot be delivered without a courier.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Logistics.ShippingAddressStreet))
            {
                _toastService.ShowError("Shipping Address Required", "Order cannot be delivered without a shipping address.");
                return;
            }

            if(Payments.DueBalance > 0)
            {
                _toastService.ShowError("Payment Required", "Order cannot be completed until full payment is received.");
                return;
            }

            string title = "Complete Order";
            string message = "Are you sure you want to mark this order as Completed?";

            bool isConfirmed = await _dialogService.ShowConfirmationAsync(title, message, "Mark as Completed", "Cancel");
            if (!isConfirmed)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await _ordersApi.CompleteOrder(_orderId.Value);
                await InitializeAsync(_orderId.Value);
                _toastService.ShowSuccess("Success", "Order has been marked as Completed.");
            }
            catch
            {
                _logger.LogError("Failed to mark as Completed order ID: {OrderId}", _orderId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task DownloadInvoiceAsync()
        {
            if (_orderId == null) return;

            try
            {
                IsBusy = true;

                var logoStoragePath = ApplicationData.Current.LocalFolder.Path + "\\StoreAssets";
                var response = await _ordersApi.GetOrderInvoiceById(_orderId.Value, logoStoragePath);

                if (response.IsSuccessStatusCode)
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    if (pdfBytes != null && pdfBytes.Length > 0)
                    {
                        // Name the file cleanly with the Order Number
                        string safeFileName = $"Invoice_{PageTitle}.pdf";
                        string tempFilePath = Path.Combine(Path.GetTempPath(), safeFileName);

                        await File.WriteAllBytesAsync(tempFilePath, pdfBytes);

                        var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(tempFilePath);
                        await Windows.System.Launcher.LaunchFileAsync(storageFile);
                    }
                }
                else
                {
                    _logger.LogWarning("API returned {StatusCode} when downloading invoice.", response.StatusCode);
                    //_toastService.ShowError("Download Failed", "Could not generate the invoice.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download invoice");
                //_toastService.ShowError("Error", "An unexpected error occurred.");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
