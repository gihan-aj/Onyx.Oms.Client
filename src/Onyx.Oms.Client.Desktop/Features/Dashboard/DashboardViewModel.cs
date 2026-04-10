using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Features.Orders;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Dashboard;

public partial class DashboardItem : ObservableObject
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public OrderStatus? OrderStatus { get; init; }
    public string IconGlyph { get; init; } = string.Empty;
    public string ItemType { get; init; } = string.Empty; // "Order" or "Task"

    public string StatusDisplay => OrderStatus.HasValue ? OrderStatus.ToString() : Status;
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;
    private readonly IDashboardApi _dashboardApi;

    private string _userName = "User";
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private string _selectedFilter = "RecentOrders";
    public string SelectedFilter
    {
        get => _selectedFilter;
        set => SetProperty(ref _selectedFilter, value);
    }

    public ObservableCollection<DashboardItem> DashboardItems { get; } = new();

    public Microsoft.UI.Xaml.Visibility EmptyListMessageVisibility => DashboardItems.Count == 0 ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

    public ObservableCollection<DashboardQuickAction> QuickActions { get; } = new();

    public IRelayCommand NavigateToCreateOrderCommand { get; }
    public IRelayCommand NavigateToAddCustomerCommand { get; }
    public IRelayCommand NavigateToCatalogCommand { get; }
    public IRelayCommand NavigateToFulfillmentCommand { get; }
    public IRelayCommand NavigateToCreateProductCommand { get; }

    public DashboardViewModel(IAuthenticationService authService, INavigationService navigationService, IDashboardApi dashboardApi)
    {
        _authService = authService;
        _navigationService = navigationService;
        _dashboardApi = dashboardApi;

        NavigateToCreateOrderCommand = new RelayCommand(NavigateToCreateOrder);
        NavigateToAddCustomerCommand = new RelayCommand(NavigateToAddCustomer);
        NavigateToCatalogCommand = new RelayCommand(NavigateToCatalog);
        NavigateToFulfillmentCommand = new RelayCommand(NavigateToFulfillment);
        NavigateToCreateProductCommand = new RelayCommand(NavigateToCreateProduct);
    }

    public void Subscribe()
    {
        _authService.AuthenticationChanged += OnAuthenticationChanged;
    }

    public void Unsubscribe()
    {
        _authService.AuthenticationChanged -= OnAuthenticationChanged;
    }

    private async void OnAuthenticationChanged(object? sender, bool isAuthenticated)
    {
        if (isAuthenticated)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                // Ensure UI updates can safely happen, standard data binding usually marshals this or handles it transparently
                UserName = currentUser.FirstName ?? currentUser.Email ?? "User";
            }
        }
        else
        {
            UserName = "User";
        }
    }

    public async Task InitializeAsync()
    {
        // Set User Name
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser != null)
        {
            UserName = currentUser.FirstName ?? currentUser.Email ?? "User";
        }

        await LoadDashboardItemsAsync();
        LoadQuickActions();
    }

    public void LoadQuickActions()
    {
        QuickActions.Clear();
        QuickActions.Add(new DashboardQuickAction("New Order", "Create a new customer order", "\uE710", NavigateToCreateOrderCommand, false, true));
        QuickActions.Add(new DashboardQuickAction("Add Customer", "Register a new buyer profile", "\uE8FA", NavigateToAddCustomerCommand, true, true));
        QuickActions.Add(new DashboardQuickAction("View Catalog", "Manage latest products/variants", "\uE81E", NavigateToCatalogCommand, true, true));
        QuickActions.Add(new DashboardQuickAction("Fulfillment", "View pending task assignments", "\uE9D5", NavigateToFulfillmentCommand, false, true));
        QuickActions.Add(new DashboardQuickAction("Create Product", "Add a new item to catalog", "\uE719", NavigateToCreateProductCommand, true, true));
    }

    private void NavigateToCreateOrder()
    {
        //_navigationService.NavigateTo(typeof(CreateOrderPage).FullName!);
    }

    private void NavigateToAddCustomer()
    {
        _navigationService.NavigateTo(typeof(Customers.CustomerFormPage).FullName!);
    }

    private void NavigateToCatalog()
    {
        _navigationService.NavigateTo(typeof(Catalog.CatalogPage).FullName!);
    }

    private void NavigateToFulfillment()
    {
        //_navigationService.NavigateTo(typeof(FulfillmentPage).FullName!);
    }

    private void NavigateToCreateProduct()
    {
        _navigationService.NavigateTo(typeof(Products.Create.CreateProductViewModel).FullName!);
    }

    public async Task LoadDashboardItemsAsync()
    {
        DashboardItems.Clear();

        try 
        {
            if (SelectedFilter == "RecentOrders")
            {
                // Placeholder - Waiting for backend API implementation
                // var orders = await _dashboardApi.GetRecentOrdersAsync();
                // foreach (var order in orders)
                // {
                //     DashboardItems.Add(order);
                // }
            }
            else if (SelectedFilter == "PendingTasks")
            {
                // Placeholder - Waiting for backend API implementation
                // var tasks = await _dashboardApi.GetPendingTasksAsync();
                // foreach (var task in tasks)
                // {
                //     DashboardItems.Add(task);
                // }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load dashboard items: {ex.Message}");
        }
        finally
        {
            OnPropertyChanged(nameof(EmptyListMessageVisibility));
        }
    }
}
