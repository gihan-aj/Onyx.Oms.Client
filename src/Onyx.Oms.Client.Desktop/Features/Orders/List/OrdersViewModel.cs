using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.List
{
    public partial class OrdersViewModel : PagedDataGridViewModelBase<OrderGridItem>, INavigationAware
    {
        private readonly IOrdersApi _ordersApi;
        private readonly IPermissionService _permissionService;
        private readonly IDialogService _dialogService;
        private readonly IToastService _toastService;
        private readonly INavigationService _navigationService;

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
        public IRelayCommand<OrderGridItem> ViewDetailsCommand { get; }
        public IRelayCommand<OrderGridItem> EditDetailsCommand { get; }
        public IRelayCommand StatusChipToggledCommand { get; }

        public OrdersViewModel(
            IOrdersApi ordersApi,
            IPermissionService permissionService,
            IDialogService dialogService,
            IToastService toastService,
            INavigationService navigationService)
        {
            _ordersApi = ordersApi;
            _permissionService = permissionService;
            _dialogService = dialogService;
            _toastService = toastService;
            _navigationService = navigationService;

            // Default Date Range: Past 30 days
            _fromDate = DateTimeOffset.Now.AddDays(-30);
            _toDate = DateTimeOffset.Now;

            InitializeFilterOptions();
            UpdateSortDefaultsForTab();

            ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
            NewOrderCommand = new RelayCommand(NavigateToNewOrder);
            ViewDetailsCommand = new RelayCommand<OrderGridItem>(ViewOrderDetails);
            EditDetailsCommand = new RelayCommand<OrderGridItem>(EditOrderDetails);
            StatusChipToggledCommand = new RelayCommand(() => 
            {
                if (SelectedTab == OrderCategoryTab.All)
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            });
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

        private void ViewOrderDetails(OrderGridItem? order)
        {
            if (order != null)
            {
                // _navigationService.NavigateTo(typeof(OrderDetailsViewModel).FullName!, order.Id);
            }
        }

        private void EditOrderDetails(OrderGridItem? order)
        {
            if (order != null)
            {
                 _navigationService.NavigateTo(typeof(EditOrderViewModel).FullName!, order.Id);
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
