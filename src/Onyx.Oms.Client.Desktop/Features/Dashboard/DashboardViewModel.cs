using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Dashboard;

public partial class DashboardItem : ObservableObject
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string IconGlyph { get; init; } = string.Empty;
    public string ItemType { get; init; } = string.Empty; // "Order" or "Task"
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

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

    public ObservableCollection<DashboardQuickAction> QuickActions { get; } = new();

    public IRelayCommand NavigateToCreateOrderCommand { get; }
    public IRelayCommand NavigateToAddCustomerCommand { get; }
    public IRelayCommand NavigateToCatalogCommand { get; }
    public IRelayCommand NavigateToFulfillmentCommand { get; }
    public IRelayCommand NavigateToCreateProductCommand { get; }

    public DashboardViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;

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

        LoadDummyData();
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

    public void LoadDummyData()
    {
        DashboardItems.Clear();

        if (SelectedFilter == "RecentOrders")
        {
            DashboardItems.Add(new DashboardItem { Title = "ORD-10042", Subtitle = "Customer: Jane Doe", Status = "Pending", IconGlyph = "\xF07A", ItemType = "Order" }); // Shopping Cart
            DashboardItems.Add(new DashboardItem { Title = "ORD-10041", Subtitle = "Customer: John Smith", Status = "Processing", IconGlyph = "\xF07A", ItemType = "Order" });
            DashboardItems.Add(new DashboardItem { Title = "ORD-10040", Subtitle = "Customer: Sarah Connor", Status = "Ready to Pack", IconGlyph = "\xF466", ItemType = "Order" }); // Box
            DashboardItems.Add(new DashboardItem { Title = "ORD-10039", Subtitle = "Customer: Kyle Reese", Status = "Shipped", IconGlyph = "\xF0D1", ItemType = "Order" }); // Truck
        }
        else if (SelectedFilter == "PendingTasks")
        {
            DashboardItems.Add(new DashboardItem { Title = "Produce: Blue T-Shirt (M)", Subtitle = "For ORD-10041 (Qty: 2)", Status = "To Be Produced", IconGlyph = "\xF015", ItemType = "Task" }); // Home/Factory
            DashboardItems.Add(new DashboardItem { Title = "Procure: Red Cap", Subtitle = "For ORD-10042 (Qty: 1)", Status = "To Be Procured", IconGlyph = "\xF291", ItemType = "Task" }); // Shopping Basket
            DashboardItems.Add(new DashboardItem { Title = "Produce: Winter Jacket (L)", Subtitle = "Stock Replenishment", Status = "In Production", IconGlyph = "\xF013", ItemType = "Task" }); // Cog
        }
    }
}
