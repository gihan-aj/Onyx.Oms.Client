using CommunityToolkit.Mvvm.ComponentModel;
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

    public DashboardViewModel(IAuthenticationService authService)
    {
        _authService = authService;
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
