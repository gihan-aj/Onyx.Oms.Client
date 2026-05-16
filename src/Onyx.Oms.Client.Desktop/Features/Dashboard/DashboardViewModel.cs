using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List;
using Onyx.Oms.Client.Desktop.Features.Orders;
using Onyx.Oms.Client.Desktop.Features.Orders.Create;
using Onyx.Oms.Client.Desktop.Features.Orders.List;
using Onyx.Oms.Client.Desktop.Features.Products.Create;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Dashboard;

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
    private string _heroSubLabel = string.Empty;
    public string HeroSubLabel
    {
        get => _heroSubLabel;
        set => SetProperty(ref _heroSubLabel, value);
    }

    private string _greeting = "Good morning,";
    public string Greeting
    {
        get => _greeting;
        set => SetProperty(ref _greeting, value);
    }

    private MainDashboardSummaryDto? _summary;
    public MainDashboardSummaryDto? Summary
    {
        get => _summary;
        set => SetProperty(ref _summary, value);
    }
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    // Wrappers to help XAML easily bind to semantic colors
    public ObservableCollection<ActionRequiredUIItem> ActionRequiredItems { get; } = new();
    public ObservableCollection<InMotionUIItem> InMotionItems { get; } = new();

    // Quick Actions
    public IRelayCommand NavigateToCreateOrderCommand { get; }
    public IRelayCommand NavigateToShipOrdersCommand { get; }
    public IRelayCommand NavigateToTasksCommand { get; }
    public IRelayCommand NavigateToCreateProductCommand { get; }
    // Stat Tiles
    public IRelayCommand NavigateToPendingOrdersCommand { get; }
    public IRelayCommand NavigateToReadyToPackCommand { get; }
    public IRelayCommand NavigateToTasksCompletedCommand { get; }
    public IRelayCommand NavigateToShippedTodayCommand { get; }

    // Refresh
    public IRelayCommand RefreshCommand { get; }

    public DashboardViewModel(IAuthenticationService authService, INavigationService navigationService, IDashboardApi dashboardApi)
    {
        _authService = authService;
        _navigationService = navigationService;
        _dashboardApi = dashboardApi;
        NavigateToCreateOrderCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(CreateOrderViewModel).FullName!));
        NavigateToShipOrdersCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(OrdersPage).FullName!, OrderStatus.ReadyToPack)); // Assuming OrderStatus filter
        NavigateToTasksCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(FulfillmentTasksPage).FullName!));
        NavigateToCreateProductCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(CreateProductViewModel).FullName!));
        NavigateToPendingOrdersCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(OrdersPage).FullName!, OrderStatus.Pending));
        NavigateToReadyToPackCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(OrdersPage).FullName!, OrderStatus.ReadyToPack));
        NavigateToTasksCompletedCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(FulfillmentTasksPage).FullName! /* Add status filter if applicable */));
        NavigateToShippedTodayCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(OrdersPage).FullName!, OrderStatus.Shipped));
        RefreshCommand = new RelayCommand(async () => await LoadDashboardItemsAsync());
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
                await InitializeAsync();
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

        if(_authService.IsAuthenticated)
            await LoadDashboardItemsAsync();
    }

    public async Task LoadDashboardItemsAsync()
    {
        IsLoading = true;
        try
        {
            var summaryTask = _dashboardApi.GetDashboardSummaryAsync();
            var actionTask = _dashboardApi.GetActionRequiredAsync(5);
            var inMotionTask = _dashboardApi.GetInMotionAsync(5);
            await Task.WhenAll(summaryTask, actionTask, inMotionTask);
            Summary = summaryTask.Result;

            var hour = DateTime.Now.Hour;
            if (hour < 12) Greeting = "Good morning,";
            else if (hour < 17) Greeting = "Good afternoon,";
            else Greeting = "Good evening,";

            // Build the date/time string with the action count
            HeroSubLabel = $"{DateTime.Now:dddd, d MMM yyyy} · {Summary.ActionRequiredCount} items need your attention";
            // Map action items for UI
            ActionRequiredItems.Clear();
            foreach (var item in actionTask.Result.Items)
            {
                ActionRequiredItems.Add(new ActionRequiredUIItem(item));
            }
            // Map in motion items for UI
            InMotionItems.Clear();
            foreach (var item in inMotionTask.Result.Items)
            {
                InMotionItems.Add(new InMotionUIItem(item));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load dashboard items: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

}
public class ActionRequiredUIItem
{
    public ActionRequiredItemDto Data { get; }

    // Default semantic colors/icons
    public string IconGlyph { get; } = "\uE711"; // Default Info icon
    public string SemanticBrushName { get; } = "SystemFillColorNeutralBrush";
    public string SemanticBackgroundName { get; } = "SystemFillColorNeutralBackgroundBrush";
    public ActionRequiredUIItem(ActionRequiredItemDto data)
    {
        Data = data;

        switch (data.Reason)
        {
            case "returned_to_sender":
            case "unpaid_balance":
                IconGlyph = "\uE8C5"; // Credit card off / warning
                SemanticBrushName = "SystemFillColorCriticalBrush";
                SemanticBackgroundName = "SystemFillColorCriticalBackgroundBrush";
                break;
            case "pending_confirmation":
            case "stalled_processing":
                IconGlyph = "\uE916"; // Clock / Pending
                SemanticBrushName = "SystemFillColorCautionBrush";
                SemanticBackgroundName = "SystemFillColorCautionBackgroundBrush";
                break;
            case "idle_ready_to_pack":
                IconGlyph = "\uE7B8"; // Package
                SemanticBrushName = "SystemFillColorSuccessBrush";
                SemanticBackgroundName = "SystemFillColorSuccessBackgroundBrush";
                break;
        }
    }
}

public class InMotionUIItem
{
    public InMotionItemDto Data { get; }

    public string DotBrushName { get; } = "SystemFillColorNeutralBrush";
    public InMotionUIItem(InMotionItemDto data)
    {
        Data = data;

        if (data.Type == "task")
        {
            if (data.IsOrphaned == true)
                DotBrushName = "SystemFillColorCautionBrush"; // Amber
            else if (data.TaskType == "Production")
                DotBrushName = "SystemFillColorSuccessBrush"; // Teal
            else if (data.TaskType == "Procurement")
                DotBrushName = "AccentTextFillColorPrimaryBrush"; // Blue
        }
        else if (data.Type == "order" && data.OrderStatus == "Shipped")
        {
            DotBrushName = "SystemFillColorSuccessBrush"; // Teal
        }
    }
}
