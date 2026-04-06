using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public partial class CouriersViewModel : ObservableObject, INavigationAware
{
    private readonly ICourierApi _courierApi;
    private readonly IToastService _toastService;
    private readonly IDialogService _dialogService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<CouriersViewModel> _logger;

    private ObservableCollection<CourierDto> _couriers = new();
    public ObservableCollection<CourierDto> Couriers
    {
        get => _couriers;
        set => SetProperty(ref _couriers, value);
    }

    private bool _isListLoading;
    public bool IsListLoading
    {
        get => _isListLoading;
        set => SetProperty(ref _isListLoading, value);
    }
    
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                // Notify commands to re-evaluate CanExecute if needed
            }
        }
    }

    private string _selectedStatus = "Active";
    public string SelectedStatus
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

    public ObservableCollection<string> StatusOptions { get; } = new(new[] { "Active", "Inactive", "All" });

    private int _page = 1;
    public int Page
    {
        get => _page;
        set
        {
            if (SetProperty(ref _page, value))
            {
                OnPropertyChanged(nameof(HasPreviousPage));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(PageSummary));
            }
        }
    }

    public List<int> PageSizes { get; } = new() { 5, 10, 20, 50 };

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                Page = 1; // Reset to first page when page size changes
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(PageSummary));
                _ = LoadDataAsync(); // Reload data with new page size
            }
        }
    }

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        set
        {
            if (SetProperty(ref _totalCount, value))
            {
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(PageSummary));
            }
        }
    }

    private string? _searchTerm;
    public string? SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    private bool _hasNoData;
    public bool HasNoData
    {
        get => _hasNoData;
        set => SetProperty(ref _hasNoData, value);
    }

    private string? _sortColumn;
    public string? SortColumn
    {
        get => _sortColumn;
        set => SetProperty(ref _sortColumn, value);
    }

    private string? _sortDirection;
    public string? SortDirection
    {
        get => _sortDirection;
        set => SetProperty(ref _sortDirection, value);
    }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public string PageSummary => TotalCount == 0 ? "No items" : $"Page {Page} of {TotalPages} ({TotalCount} items)";

    public IAsyncRelayCommand LoadDataCommand { get; }
    public IAsyncRelayCommand NextPageCommand { get; }
    public IAsyncRelayCommand PreviousPageCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand ClearFiltersCommand { get; }
    public IAsyncRelayCommand<string> SearchCommand { get; }
    public IAsyncRelayCommand<CourierDto> DeleteCommand { get; }
    public IAsyncRelayCommand<CourierDto> ActivateCommand { get; }
    public IAsyncRelayCommand<CourierDto> DeactivateCommand { get; }

    public CouriersViewModel(
        ICourierApi courierApi, 
        IToastService toastService, 
        IDialogService dialogService,
        IPermissionService permissionService,
        ILogger<CouriersViewModel> logger)
    {
        _courierApi = courierApi;
        _toastService = toastService;
        _dialogService = dialogService;
        _permissionService = permissionService;
        _logger = logger;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        NextPageCommand = new AsyncRelayCommand(OnNextPage);
        PreviousPageCommand = new AsyncRelayCommand(OnPreviousPage);
        RefreshCommand = new AsyncRelayCommand(OnRefresh);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFlitersAsync);
        SearchCommand = new AsyncRelayCommand<string>(OnSearch);
        DeleteCommand = new AsyncRelayCommand<CourierDto>(DeleteCourier);
        ActivateCommand = new AsyncRelayCommand<CourierDto>(ActivateCourier);
        DeactivateCommand = new AsyncRelayCommand<CourierDto>(DeactivateCourier);
    }

    private async Task LoadDataAsync()
    {
        if (IsListLoading) return;

        try
        {
            IsListLoading = true;
            HasNoData = false;
            
            bool? activeFilter = SelectedStatus switch
            {
                "Active" => true,
                "Inactive" => false,
                _ => null,
            };

            var result = await _courierApi.SearchCouriers(Page, PageSize, SearchTerm, SortColumn, SortDirection, activeFilter);
            
            var canView = _permissionService.CanExecute(Shared.Constants.Permissions.Couriers.View);
            var canEdit = _permissionService.CanExecute(Shared.Constants.Permissions.Couriers.Edit);
            var canDelete = _permissionService.CanExecute(Shared.Constants.Permissions.Couriers.Delete);
            var canActivate = _permissionService.CanExecute(Shared.Constants.Permissions.Couriers.Activate);
            var canDeactivate = _permissionService.CanExecute(Shared.Constants.Permissions.Couriers.Deactivate);

            Couriers.Clear();
            foreach (var item in result.Items)
            {
                item.CanView = canView;
                item.CanEdit = canEdit;
                item.CanDelete = canDelete;
                item.CanToggleStatus = item.IsActive ? canDeactivate : canActivate;
                Couriers.Add(item);
            }

            TotalCount = result.TotalCount;
            HasNoData = TotalCount == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading couriers: {ex.Message}");
            HasNoData = true; 
        }
        finally
        {
            IsListLoading = false;
        }
    }


    public async Task DeleteCourier(CourierDto? courier)
    {
        if (courier == null || IsBusy) return;

        try
        {
            IsBusy = true;
            await _courierApi.DeleteCourier(courier.Id);
            _toastService.ShowSuccess("Success", "Courier deleted successfully.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting courier: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ActivateCourier(CourierDto? courier)
    {
        if (courier == null || IsBusy) return;

        try
        {
            IsBusy = true;
            await _courierApi.ActivateCourier(courier.Id);
            _toastService.ShowSuccess("Success", "Courier activated successfully.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error activating courier: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DeactivateCourier(CourierDto? courier)
    {
        if (courier == null || IsBusy) return;

        try
        {
            IsBusy = true;
            await _courierApi.DeactivateCourier(courier.Id);
            _toastService.ShowSuccess("Success", "Courier deactivated successfully.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deactivating courier: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task Sort(string column, string direction)
    {
        SortColumn = column;
        SortDirection = direction;
        await LoadDataAsync();
    }

    private async Task OnNextPage()
    {
        if (HasNextPage)
        {
            Page++;
            await LoadDataAsync();
        }
    }

    private async Task OnPreviousPage()
    {
        if (HasPreviousPage)
        {
            Page--;
            await LoadDataAsync();
        }
    }

    private async Task OnRefresh()
    {
        Page = 1;
        SearchTerm = null;
        SortColumn = null;
        SortDirection = null;
        SelectedStatus = "Active";
        await LoadDataAsync();
    }

    private async Task ClearFlitersAsync()
    {
        SearchTerm = string.Empty;
        SelectedStatus = "All";
        Page = 1;
        SortColumn = null;
        SortDirection = null;
        await LoadDataAsync();
    }

    private async Task OnSearch(string? query)
    {
        SearchTerm = query;
        Page = 1;
        await LoadDataAsync();
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadDataAsync();
    }

    public void OnNavigatedFrom()
    {
        // Cleanup if needed
    }
}
