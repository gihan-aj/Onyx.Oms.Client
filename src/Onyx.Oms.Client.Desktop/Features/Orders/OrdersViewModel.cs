using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders;

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
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
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
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
            }
        }
    }

    private OrderStatusOption? _selectedStatus;
    public OrderStatusOption? SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
            {
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
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
                Page = 1;
                LoadDataCommand.ExecuteAsync(null);
            }
        }
    }

    public ObservableCollection<OrderStatusOption> StatusOptions { get; } = new();
    public ObservableCollection<PaymentStatusOption> PaymentStatusOptions { get; } = new();

    // --- Permissions ---
    public bool CanCreateOrders => _permissionService.CanExecute(Permissions.Orders.Create);
    public bool CanEditOrders => _permissionService.CanExecute(Permissions.Orders.Edit);
    public bool CanViewOrders => _permissionService.CanExecute(Permissions.Orders.View);

    // -- Commands --
    public IAsyncRelayCommand ClearFiltersCommand { get; }
    public IRelayCommand NewOrderCommand { get; }
    public IRelayCommand<OrderGridItem> ViewDetailsCommand { get; }
    public IRelayCommand<OrderGridItem> EditDetailsCommand { get; }

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

        InitializeFilterOptions();

        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
        NewOrderCommand = new RelayCommand(NavigateToNewOrder);
        ViewDetailsCommand = new RelayCommand<OrderGridItem>(ViewOrderDetails);
        EditDetailsCommand = new RelayCommand<OrderGridItem>(EditOrderDetails);
    }

    private void InitializeFilterOptions()
    {
        StatusOptions.Add(new OrderStatusOption("All Statuses", null));
        foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
        {
            StatusOptions.Add(new OrderStatusOption(status.ToString(), status));
        }
        _selectedStatus = StatusOptions[0];

        PaymentStatusOptions.Add(new PaymentStatusOption("All Payment Statuses", null));
        foreach (PaymentStatus status in Enum.GetValues(typeof(PaymentStatus)))
        {
            PaymentStatusOptions.Add(new PaymentStatusOption(status.ToString(), status));
        }
        _selectedPaymentStatus = PaymentStatusOptions[0];
    }

    public void OnNavigatedFrom()
    {
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadDataAsync();
    }

    protected override async Task LoadDataAsync()
    {
        if (IsListLoading)
            return;

        try
        {
            IsListLoading = true;

            var result = await _ordersApi.GetOrdersPaged(
                page: Page,
                pageSize: PageSize,
                searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                sortColumn: SortColumn,
                sortOrder: SortOrder,
                status: SelectedStatus?.Value,
                paymentStatus: SelectedPaymentStatus?.Value,
                customerId: SelectedCustomer?.Id);

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
        SelectedStatus = StatusOptions[0];
        SelectedPaymentStatus = PaymentStatusOptions[0];
        return Task.CompletedTask;
    }

    private async Task ClearFiltersAsync()
    {
        SearchTerm = string.Empty;
        SelectedCustomer = null;
        SelectedStatus = StatusOptions[0];
        SelectedPaymentStatus = PaymentStatusOptions[0];
        Page = 1;
        await LoadDataAsync();
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
        // _navigationService.NavigateTo(typeof(CreateOrderViewModel).FullName!);
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
            // _navigationService.NavigateTo(typeof(EditOrderViewModel).FullName!, order.Id);
        }
    }
}

public record OrderStatusOption(string DisplayName, OrderStatus? Value);
public record PaymentStatusOption(string DisplayName, PaymentStatus? Value);
