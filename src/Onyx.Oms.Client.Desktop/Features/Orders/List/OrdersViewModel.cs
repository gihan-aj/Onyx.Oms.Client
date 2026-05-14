using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Features.Orders.Create;
using Onyx.Oms.Client.Desktop.Features.Orders.Edit;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Onyx.Oms.Client.Desktop.Features.Orders.List
{
    public partial class OrdersViewModel : PagedDataGridViewModelBase<OrderGridItem>, INavigationAware
    {
        private readonly IOrdersApi _ordersApi;
        private readonly IPermissionService _permissionService;
        private readonly IDialogService _dialogService;
        private readonly IToastService _toastService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<OrdersViewModel> _logger;

        // UI 
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // -- Filtering --
        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetProperty(ref _searchTerm, value))
                {
                    _ = ReloadDataAndCountsAsync();
                }
            }
        }

        private CustomerDto? _selectedCustomer;
        public CustomerDto? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    _ = ReloadDataAndCountsAsync();
                }
            }
        }

        private PaymentStatusOption? _selectedPaymentStatus;
        public PaymentStatusOption? SelectedPaymentStatus
        {
            get => _selectedPaymentStatus;
            set
            {
                if (SetProperty(ref _selectedPaymentStatus, value))
                {
                    _ = ReloadDataAndCountsAsync();
                }
            }
        }

        private DateTimeOffset? _fromDate;
        public DateTimeOffset? FromDate
        {
            get => _fromDate;
            set
            {
                if (SetProperty(ref _fromDate, value))
                {
                    _ = ReloadDataAndCountsAsync();
                }
            }
        }

        private DateTimeOffset? _toDate;
        public DateTimeOffset? ToDate
        {
            get => _toDate;
            set
            {
                if (SetProperty(ref _toDate, value))
                {
                    _ = ReloadDataAndCountsAsync();
                }
            }
        }

        private IsCashOnDeliveryOption? _selectedIsCod;
        public IsCashOnDeliveryOption? SelectedIsCod
        {
            get => _selectedIsCod;
            set
            {
                if (SetProperty(ref _selectedIsCod, value))
                {
                    _ = ReloadDataAndCountsAsync();
                }
            }
        }

        private CourierDto? _selectedCourier;
        public CourierDto? SelectedCourier
        {
            get => _selectedCourier;
            set
            {
                if (SetProperty(ref _selectedCourier, value))
                {
                    _ = ReloadDataAndCountsAsync();
                }
            }
        }

        private OrderCategoryTab _selectedTab = OrderCategoryTab.All;
        public OrderCategoryTab SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                {
                    UpdateSortDefaultsForTab();
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        public ObservableCollection<PaymentStatusOption> PaymentStatusOptions { get; } = new();
        public ObservableCollection<IsCashOnDeliveryOption> IsCodOptions { get; } = new();
        public ObservableCollection<CourierDto> Couriers { get; } = new();
        public ObservableCollection<StatusChipViewModel> StatusChips { get; } = new();

        private string _allActiveTabText = "All";
        public string AllActiveTabText { get => _allActiveTabText; set => SetProperty(ref _allActiveTabText, value); }

        private string _pendingTabText = "Pending";
        public string PendingTabText { get => _pendingTabText; set => SetProperty(ref _pendingTabText, value); }

        private string _processingTabText = "Processing";
        public string ProcessingTabText { get => _processingTabText; set => SetProperty(ref _processingTabText, value); }

        private string _readyToPackTabText = "Ready to Pack";
        public string ReadyToPackTabText { get => _readyToPackTabText; set => SetProperty(ref _readyToPackTabText, value); }

        private string _packedTabText = "Packed";
        public string PackedTabText { get => _packedTabText; set => SetProperty(ref _packedTabText, value); }

        private string _historicalTabText = "Historical";
        public string HistoricalTabText { get => _historicalTabText; set => SetProperty(ref _historicalTabText, value); }

        // --- Permissions ---
        public bool CanCreateOrders => _permissionService.CanExecute(Permissions.Orders.Create);
        public bool CanEditOrders => _permissionService.CanExecute(Permissions.Orders.Edit);
        public bool CanViewOrders => _permissionService.CanExecute(Permissions.Orders.View);

        // -- Commands --
        public IAsyncRelayCommand ClearFiltersCommand { get; }
        public IRelayCommand NewOrderCommand { get; }
        public IRelayCommand<OrderGridItem> ManageOrderCommand { get; }
        public IRelayCommand<OrderGridItem> ConfirmOrderCommand { get; }
        public IRelayCommand<OrderGridItem> CancelOrderCommand { get; }
        public IRelayCommand<OrderGridItem> PackOrderCommand { get; }
        public IRelayCommand<OrderGridItem> ShipOrderCommand { get; }
        public IRelayCommand<OrderGridItem> DeliverOrderCommand { get; }
        public IRelayCommand<OrderGridItem> CompleteOrderCommand { get; }
        public IRelayCommand<OrderGridItem> FailDeliveryCommand { get; }
        public IRelayCommand StatusChipToggledCommand { get; }
        public IAsyncRelayCommand BulkPrintShippingLabelsCommand { get; }

        public OrdersViewModel(
            IOrdersApi ordersApi,
            IPermissionService permissionService,
            IDialogService dialogService,
            IToastService toastService,
            INavigationService navigationService,
            ILogger<OrdersViewModel> logger)
        {
            _ordersApi = ordersApi;
            _permissionService = permissionService;
            _dialogService = dialogService;
            _toastService = toastService;
            _navigationService = navigationService;
            _logger = logger;

            // Default Date Range: Past 30 days
            _fromDate = DateTimeOffset.Now.AddDays(-30);
            _toDate = DateTimeOffset.Now;

            InitializeFilterOptions();
            UpdateSortDefaultsForTab();

            ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
            NewOrderCommand = new RelayCommand(NavigateToNewOrder);
            ManageOrderCommand = new RelayCommand<OrderGridItem>(ManageOrder);
            ConfirmOrderCommand = new RelayCommand<OrderGridItem>(ConfirmOrder);
            CancelOrderCommand = new RelayCommand<OrderGridItem>(CancelOrder);
            PackOrderCommand = new RelayCommand<OrderGridItem>(PackOrder);
            ShipOrderCommand = new RelayCommand<OrderGridItem>(ShipOrder);
            DeliverOrderCommand = new RelayCommand<OrderGridItem>(DeliverOrder);
            CompleteOrderCommand = new RelayCommand<OrderGridItem>(CompleteOrder);
            FailDeliveryCommand = new RelayCommand<OrderGridItem>(FailDelivery);
            StatusChipToggledCommand = new RelayCommand(() =>
            {
                if (SelectedTab == OrderCategoryTab.All)
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            });
            BulkPrintShippingLabelsCommand = new AsyncRelayCommand(BulkPrintShippingLabelsAsync);
        }

        private void InitializeFilterOptions()
        {
            PaymentStatusOptions.Add(new PaymentStatusOption("All Payment Statuses", null));
            foreach (PaymentStatus status in Enum.GetValues(typeof(PaymentStatus)))
            {
                PaymentStatusOptions.Add(new PaymentStatusOption(status.ToString(), status));
            }
            _selectedPaymentStatus = PaymentStatusOptions[0];

            IsCodOptions.Add(new IsCashOnDeliveryOption("All", null));
            IsCodOptions.Add(new IsCashOnDeliveryOption("Yes", true));
            IsCodOptions.Add(new IsCashOnDeliveryOption("No", false));
            _selectedIsCod = IsCodOptions[0];

            // Initialize Chips
            var activeStatuses = new[] 
            { 
                OrderStatus.Pending, 
                OrderStatus.Confirmed, 
                OrderStatus.Processing, 
                OrderStatus.ReadyToPack, 
                OrderStatus.Packed, 
                OrderStatus.PaymentFailed 
            };

            foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
            {
                var chip = new StatusChipViewModel(status.ToString(), status, activeStatuses.Contains(status));
                chip.PropertyChanged += (s, e) => {
                    if (e.PropertyName == nameof(StatusChipViewModel.IsChecked))
                    {
                        StatusChipToggledCommand.Execute(null);
                    }
                };
                StatusChips.Add(chip);
            }
        }

        private void UpdateSortDefaultsForTab()
        {
            if (SelectedTab == OrderCategoryTab.Historical)
            {
                SortColumn = "OrderDate";
                SortOrder = "DESC";
            }
            else
            {
                SortColumn = "OrderDate";
                SortOrder = "ASC";
            }
        }

        public void OnNavigatedFrom()
        {
        }

        public async void OnNavigatedTo(object parameter)
        {
            await LoadCouriersAsync();
            await ReloadDataAndCountsAsync();
        }

        private async Task LoadCouriersAsync()
        {
            try
            {
                var couriers = await _ordersApi.GetCouriers(isActive: true);
                Couriers.Clear();
                foreach (var c in couriers)
                {
                    Couriers.Add(c);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading couriers: {ex.Message}");
            }
        }

        private async Task ReloadDataAndCountsAsync()
        {
            Page = 1;
            await LoadCountsAsync();
            await LoadDataCommand.ExecuteAsync(null);
        }

        private async Task LoadCountsAsync()
        {
            try
            {
                var result = await _ordersApi.GetOrderStatusCount(
                    searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                    paymentStatus: SelectedPaymentStatus?.Value,
                    customerId: SelectedCustomer?.Id,
                    courierId: SelectedCourier?.Id,
                    isCashOnDelivery: SelectedIsCod?.Value,
                    fromDate: FromDate,
                    toDate: ToDate);

                var counts = result.Counts;

                int pendingCount = counts.Where(c => c.Status == OrderStatus.Pending).Sum(c => c.Count);
                int processingCount = counts.Where(c => c.Status == OrderStatus.Processing).Sum(c => c.Count);
                int readyToPackCount = counts.Where(c => c.Status == OrderStatus.ReadyToPack).Sum(c => c.Count);
                int packedCount = counts.Where(c => c.Status == OrderStatus.Packed).Sum(c => c.Count);         

                var historicalStatuses = new[] { OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Completed, OrderStatus.Cancelled };
                int historicalCount = counts.Where(c => historicalStatuses.Contains(c.Status)).Sum(c => c.Count);

                AllActiveTabText = $"All ({result.TotalCount})";
                PendingTabText = $"Pending ({pendingCount})";
                ProcessingTabText = $"Processing ({processingCount})";
                ReadyToPackTabText = $"Ready to Pack ({readyToPackCount})";
                PackedTabText = $"Packed ({packedCount})";
                HistoricalTabText = $"Historical ({historicalCount})";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading counts: {ex.Message}");
            }
        }

        protected override async Task LoadDataAsync()
        {
            if (IsListLoading)
                return;

            try
            {
                IsListLoading = true;

                OrderStatus[]? statusesToFetch = null;

                switch (SelectedTab)
                {
                    case OrderCategoryTab.All:
                        statusesToFetch = StatusChips.Where(c => c.IsChecked).Select(c => c.Value).ToArray();
                        if (statusesToFetch.Length == 0)
                        {
                            Items.Clear();
                            TotalCount = 0;
                            return;
                        }
                        break;
                    case OrderCategoryTab.Pending:
                        statusesToFetch = new[] { OrderStatus.Pending };
                        break;
                    case OrderCategoryTab.Processing:
                        statusesToFetch = new[] { OrderStatus.Processing };
                        break;
                    case OrderCategoryTab.ReadyToPack:
                        statusesToFetch = new[] { OrderStatus.ReadyToPack };
                        break;
                    case OrderCategoryTab.Packed:
                        statusesToFetch = new[] { OrderStatus.Packed };
                        break;
                    case OrderCategoryTab.Historical:
                        statusesToFetch = new[] { OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Completed, OrderStatus.Cancelled };
                        break;
                }

                var result = await _ordersApi.GetOrdersPaged(
                    page: Page,
                    pageSize: PageSize,
                    searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                    sortColumn: SortColumn,
                    sortOrder: SortOrder,
                    statuses: statusesToFetch,
                    paymentStatus: SelectedPaymentStatus?.Value,
                    customerId: SelectedCustomer?.Id,
                    courierId: SelectedCourier?.Id,
                    isCashOnDelivery: SelectedIsCod?.Value,
                    fromDate: FromDate,
                    toDate: ToDate);

                Items.Clear();

                foreach (var item in result.Items)
                {
                    var gridItem = item.ToGridItem(CanEditOrders, CanViewOrders);
                    Items.Add(gridItem);
                }

                Page = result.Page;
                TotalCount = result.TotalCount;
                HasNextPage = result.HasNextPage;
                HasPreviousPage = result.HasPreviousPage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading orders: {ex.Message}");
            }
            finally
            {
                IsListLoading = false;
            }
        }

        protected override Task OnRefreshFiltersAsync()
        {
            SearchTerm = string.Empty;
            SelectedCustomer = null;
            SelectedPaymentStatus = PaymentStatusOptions[0];
            SelectedIsCod = IsCodOptions[0];
            SelectedCourier = null;
            FromDate = DateTimeOffset.Now.AddDays(-30);
            ToDate = DateTimeOffset.Now;
            foreach (var chip in StatusChips)
            {
                chip.IsChecked = true;
            }
            SelectedTab = OrderCategoryTab.All;
            return Task.CompletedTask;
        }

        private async Task ClearFiltersAsync()
        {
            await OnRefreshFiltersAsync();
            await ReloadDataAndCountsAsync();
        }

        public async Task<PagedResult<CustomerDto>> FetchCustomersAsync(string searchTerm, int page, int pageSize, CancellationToken token = default)
        {
            try
            {
                return await _ordersApi.SearchCustomers(
                    page: page,
                    pageSize: pageSize,
                    searchTerm: string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                    isActive: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching customers: {ex.Message}");
                return new PagedResult<CustomerDto> { Items = new(), Page = page, PageSize = pageSize, TotalCount = 0 };
            }
        }

        private void NavigateToNewOrder()
        {
             _navigationService.NavigateTo(typeof(CreateOrderViewModel).FullName!);
        }

        private void ManageOrder(OrderGridItem? order)
        {
            if (order != null)
            {
                 _navigationService.NavigateTo(typeof(EditOrderViewModel).FullName!, order.Id);
            }
        }

        public async Task DownloadInvoiceAsync(OrderGridItem? order)
        {
            if (order == null) return;

            try
            {
                IsBusy = true;

                var logoStoragePath = ApplicationData.Current.LocalFolder.Path + "\\StoreAssets";
                var response = await _ordersApi.GetOrderInvoiceById(order.Id, logoStoragePath);

                if (response.IsSuccessStatusCode)
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    if (pdfBytes != null && pdfBytes.Length > 0)
                    {
                        // Name the file cleanly with the Order Number
                        string safeFileName = $"Invoice_{order.OrderNumber}.pdf";
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

        public async Task DownloadShippingLabelAsync(OrderGridItem? order)
        {
            if (order == null) return;

            try
            {
                IsBusy = true;

                var response = await _ordersApi.GetShippingLabelById(order.Id);

                if (response.IsSuccessStatusCode)
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    if (pdfBytes != null && pdfBytes.Length > 0)
                    {
                        // Name the file cleanly with the Order Number
                        string safeFileName = $"Shipping_Label_{order.OrderNumber}.pdf";
                        string tempFilePath = Path.Combine(Path.GetTempPath(), safeFileName);

                        await File.WriteAllBytesAsync(tempFilePath, pdfBytes);

                        var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(tempFilePath);
                        await Windows.System.Launcher.LaunchFileAsync(storageFile);
                    }
                }
                else
                {
                    _logger.LogWarning("API returned {StatusCode} when downloading shipping label.", response.StatusCode);
                    //_toastService.ShowError("Download Failed", "Could not generate the invoice.");
                }
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to download shipping label");
                //_toastService.ShowError("Error", "An unexpected error occurred.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task BulkPrintShippingLabelsAsync()
        {
            var dialog = new BulkPrintShippingLabelsDialog(_ordersApi)
            {
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            var result = await dialog.ShowAsync();
            // If the user clicks Generate Labels and they actually have items in the list
            if (result == ContentDialogResult.Primary && dialog.SelectedOrderIds.Any())
            {
                try
                {
                    IsBusy = true;

                    // Adjust this class name to whatever you named your DTO
                    var request = new BulkGenerateShippingLabelsCommand(dialog.SelectedOrderIds);

                    var response = await _ordersApi.GetBulkShippingLabels(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                        if (pdfBytes != null && pdfBytes.Length > 0)
                        {
                            string safeFileName = $"Bulk_Shipping_Labels_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                            string tempFilePath = Path.Combine(Path.GetTempPath(), safeFileName);
                            await File.WriteAllBytesAsync(tempFilePath, pdfBytes);
                            var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(tempFilePath);
                            await Windows.System.Launcher.LaunchFileAsync(storageFile);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("API returned {StatusCode} when bulk downloading shipping labels.", response.StatusCode);
                        //_toastService.ShowError("Download Failed", "Could not generate bulk labels. Please check the backend logs.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to bulk generate shipping labels.");
                    //_toastService.ShowError("Error", "An unexpected error occurred while generating bulk labels.");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
        private async void ConfirmOrder(OrderGridItem? order) 
        {
            if (order == null)
                return;

            try
            {
                IsBusy = true;
                var orderDetails = await _ordersApi.GetOrderById(order.Id);
                if(orderDetails == null)
                {
                    _toastService.ShowError("Error", "Order details could not be loaded.");
                    return;
                }
                if (!orderDetails.Items.Any())
                {
                    _toastService.ShowError("Cannot confirm order", "This order has no items.");
                    return;
                }

                bool hasStockShortage = orderDetails.Items.Any(item => item.Quantity > item.AvailableQuantity);

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

                await _ordersApi.ConfirmOrder(orderDetails.Id);
                await LoadCountsAsync();
                await LoadDataCommand.ExecuteAsync(null);
                _toastService.ShowSuccess("Success", $"Order: {order.OrderNumber} has been confirmed.");
            }
            catch
            {
                _logger.LogError("Confirmation process failed for order ID: {OrderId}", order.Id);
            }
            finally
            {
                IsBusy = false;
            }

        }
        private async void CancelOrder(OrderGridItem? order) 
        {
            if (order == null)
                return;

            try
            {
                IsBusy = true;
                var orderDetails = await _ordersApi.GetOrderById(order.Id);
                if (orderDetails == null)
                {
                    _toastService.ShowError("Error", "Order details could not be loaded.");
                    return;
                }
                bool hasAllocatedItems = orderDetails.Items.Any(item => item.AllocatedQuantity > 0);

                var dialog = new CancelDeliveryDialog(hasAllocatedItems)
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                {
                    return;
                }

                var command = new CancelOrderCommand(dialog.Reason);
                await _ordersApi.CancelOrder(orderDetails.Id, command);
                await LoadCountsAsync();
                await LoadDataCommand.ExecuteAsync(null);
                _toastService.ShowSuccess("Success", $"Order: {order.OrderNumber} has been cancelled.");
            }
            catch
            {
                _logger.LogError("Cancel process failed for order ID: {OrderId}", order.Id);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async void PackOrder(OrderGridItem? order) 
        {
            if (order == null)
                return;

            try
            {
                IsBusy = true;
                var orderDetails = await _ordersApi.GetOrderById(order.Id);
                if (orderDetails == null)
                {
                    _toastService.ShowError("Error", "Order details could not be loaded.");
                    return;
                }
                if (!orderDetails.Items.Any())
                {
                    _toastService.ShowError("Cannot pack order", "This order has no items.");
                    return;
                }

                bool hasPendingItems = orderDetails.Items.Any(item => item.PendingQuantity > 0);
                if (hasPendingItems)
                {
                    _toastService.ShowError("Cannot pack order", "Some items have not been allocated yet.");
                    return;
                }

                bool isConfirmed = await _dialogService.ShowConfirmationAsync(
                    "Pack Order", 
                    "Are you sure you want to mark this order as Packed?", 
                    "Pack Order", "Cancel");
                if (!isConfirmed)
                {
                    return;
                }

                await _ordersApi.PackOrder(orderDetails.Id);
                await LoadCountsAsync();
                await LoadDataCommand.ExecuteAsync(null);
                _toastService.ShowSuccess("Success", $"Order: {order.OrderNumber} has been marked as packed.");
            }
            catch
            {
                _logger.LogError("Marking as packed failed for order ID: {OrderId}", order.Id);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async void ShipOrder(OrderGridItem? order) 
        {
            if (order == null)
                return;

            try
            {
                IsBusy = true;
                var orderDetails = await _ordersApi.GetOrderById(order.Id);
                if (orderDetails == null)
                {
                    _toastService.ShowError("Error", "Order details could not be loaded.");
                    return;
                }
                if (!orderDetails.Items.Any())
                {
                    _toastService.ShowError("Cannot ship order", "This order has no items.");
                    return;
                }

                bool hasPendingItems = orderDetails.Items.Any(item => item.PendingQuantity > 0);
                if (hasPendingItems)
                {
                    _toastService.ShowError("Cannot ship order", "Some items have not been allocated yet.");
                    return;
                }

                string? courierName = Couriers.FirstOrDefault(c => c.Id == orderDetails.CourierId)?.Name;

                string? address = null;
                if (!string.IsNullOrWhiteSpace(orderDetails.ShippingAddressStreet))
                {
                    address = $"{orderDetails.ShippingAddressStreet}, {orderDetails.ShippingAddressCity}, {orderDetails.ShippingAddressDistrict}";
                }

                var dialog = new ShippingConfirmationDialog(courierName, address)
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary && orderDetails.CourierId.HasValue)
                {
                    var command = new ShipOrderCommand(orderDetails.CourierId.Value, dialog.TrackingNumber);
                    await _ordersApi.ShipOrder(orderDetails.Id, command);
                    await LoadCountsAsync();
                    await LoadDataCommand.ExecuteAsync(null);
                    _toastService.ShowSuccess("Success", $"Order: {order.OrderNumber} has been marked as shipped.");
                }
            }
            catch
            {
                _logger.LogError("Marking as shipped failed for order ID: {OrderId}", order.Id);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async void DeliverOrder(OrderGridItem? order) 
        {
            if (order == null)
                return;
            try
            {
                IsBusy = true;
                var orderDetails = await _ordersApi.GetOrderById(order.Id);
                if (orderDetails == null)
                {
                    _toastService.ShowError("Error", "Order details could not be loaded.");
                    return;
                }
                if (!orderDetails.Items.Any())
                {
                    _toastService.ShowError("Cannot deliver order", "This order has no items.");
                    return;
                }

                bool hasPendingItems = orderDetails.Items.Any(item => item.PendingQuantity > 0);
                if (hasPendingItems)
                {
                    _toastService.ShowError("Cannot deliver order", "Some items have not been allocated yet.");
                    return;
                }

                if (!orderDetails.CourierId.HasValue)
                {
                    _toastService.ShowError("Courier Required", "Order cannot be delivered without a courier.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(orderDetails.ShippingAddressStreet))
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

                await _ordersApi.DeliverOrder(orderDetails.Id);
                await LoadCountsAsync();
                await LoadDataCommand.ExecuteAsync(null);
                _toastService.ShowSuccess("Success", $"Order: {order.OrderNumber} has been marked as delivered.");
            }
            catch
            {
                _logger.LogError("Marking as delivered failed for order ID: {OrderId}", order.Id);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async void CompleteOrder(OrderGridItem? order) 
        {
            if (order == null)
                return;

            try
            {
                IsBusy = true;
                var orderDetails = await _ordersApi.GetOrderById(order.Id);
                if (orderDetails == null)
                {
                    _toastService.ShowError("Error", "Order details could not be loaded.");
                    return;
                }
                if (!orderDetails.Items.Any())
                {
                    _toastService.ShowError("Cannot complete order", "This order has no items.");
                    return;
                }

                bool hasPendingItems = orderDetails.Items.Any(item => item.PendingQuantity > 0);
                if (hasPendingItems)
                {
                    _toastService.ShowError("Cannot complete order", "Some items have not been allocated yet.");
                    return;
                }

                if (!orderDetails.CourierId.HasValue)
                {
                    _toastService.ShowError("Courier Required", "Order cannot be delivered without a courier.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(orderDetails.ShippingAddressStreet))
                {
                    _toastService.ShowError("Shipping Address Required", "Order cannot be delivered without a shipping address.");
                    return;
                }

                if(orderDetails.PaymentStatus != PaymentStatus.FullyPaid || orderDetails.BalanceAmount > 0)
                {
                    _toastService.ShowError("Cannot complete order", $"There is an outstanding balance of {orderDetails.BaseCurrency} {orderDetails.BalanceAmount}.");
                    return;
                }

                string title = "Complete Order";
                string message = "Are you sure you want to mark this order as Completed?";

                bool isConfirmed = await _dialogService.ShowConfirmationAsync(title, message, "Mark as Completed", "Cancel");
                if (!isConfirmed)
                {
                    return;
                }

                await _ordersApi.CompleteOrder(orderDetails.Id);
                await LoadCountsAsync();
                await LoadDataCommand.ExecuteAsync(null);
                _toastService.ShowSuccess("Success", $"Order: {order.OrderNumber} has been marked as completed.");

            }
            catch
            {
                _logger.LogError("Marking as completed failed for order ID: {OrderId}", order.Id);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async void FailDelivery(OrderGridItem? order) 
        {
            if (order == null)
                return;

            try
            {
                IsBusy = true;
                var orderDetails = await _ordersApi.GetOrderById(order.Id);
                if (orderDetails == null)
                {
                    _toastService.ShowError("Error", "Order details could not be loaded.");
                    return;
                }

                var dialog = new FailDeliveryDialog()
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                {
                    return;
                }

                var command = new FailDeliveryCommand(dialog.IsReturnedToSender, dialog.Reason);
                await _ordersApi.FailOrderDelivery(orderDetails.Id, command);
                await LoadCountsAsync();
                await LoadDataCommand.ExecuteAsync(null);
                _toastService.ShowSuccess("Success", $"Order: {order.OrderNumber} delivery has been marked as failed.");
            }
            catch
            {
                _logger.LogError("Marking delivery as failed failed for order ID: {OrderId}", order.Id);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public partial class StatusChipViewModel : ObservableObject
    {
        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        private OrderStatus _value;
        public OrderStatus Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        public StatusChipViewModel(string displayName, OrderStatus value, bool isChecked)
        {
            DisplayName = displayName;
            Value = value;
            IsChecked = isChecked;
        }
    }

    public record PaymentStatusOption(string DisplayName, PaymentStatus? Value);
    public record IsCashOnDeliveryOption(string DisplayName, bool? Value);

    public enum OrderCategoryTab
    {
        All,
        Pending,
        Processing,
        ReadyToPack,
        Packed,
        Historical
    }
}
