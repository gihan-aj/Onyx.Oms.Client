using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    private ObservableCollection<CourierDto> _couriers = new();
    public ObservableCollection<CourierDto> Couriers
    {
        get => _couriers;
        set => SetProperty(ref _couriers, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

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
    public IAsyncRelayCommand<string> SearchCommand { get; }

    public CouriersViewModel(ICourierApi courierApi, IToastService toastService, IDialogService dialogService)
    {
        _courierApi = courierApi;
        _toastService = toastService;
        _dialogService = dialogService;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        NextPageCommand = new AsyncRelayCommand(OnNextPage);
        PreviousPageCommand = new AsyncRelayCommand(OnPreviousPage);
        RefreshCommand = new AsyncRelayCommand(OnRefresh);
        SearchCommand = new AsyncRelayCommand<string>(OnSearch);
    }

    private async Task LoadDataAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            HasNoData = false;
            
            var result = await _courierApi.SearchCouriers(Page, PageSize, SearchTerm, SortColumn, SortDirection);
            
            Couriers.Clear();
            foreach (var item in result.Items)
            {
                Couriers.Add(item);
            }

            TotalCount = result.TotalCount;
            HasNoData = TotalCount == 0;
            
            // Update dependent properties manually since we are not using [ObservableProperty] for them anymore?
            // Actually PageSummary depends on TotalCount which we updated via property setter, so it should trigger.
        }
        catch (Exception ex)
        {
            // Error handling is mostly done by the DelegatingHandler, but we catch here to stop loading spinner
            // and potentially log. The Interceptor shows the toast.
            Console.WriteLine($"Error loading couriers: {ex.Message}");
            HasNoData = true; // Or show specific error state
        }
        finally
        {
            IsLoading = false;
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
