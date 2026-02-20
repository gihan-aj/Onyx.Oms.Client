using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Roles;

public partial class RolesViewModel : ObservableObject, INavigationAware
{
    private readonly IRoleApi _roleApi;
    private readonly IToastService _toastService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<RolesViewModel> _logger;

    private ObservableCollection<RoleDto> _roles = new();
    public ObservableCollection<RoleDto> Roles
    {
        get => _roles;
        set => SetProperty(ref _roles, value);
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
        set => SetProperty(ref _isBusy, value);
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
                Page = 1;
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(PageSummary));
                _ = LoadDataAsync();
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
    public IAsyncRelayCommand<RoleDto> DeleteCommand { get; }
    public IAsyncRelayCommand<RoleDto> ActivateCommand { get; }
    public IAsyncRelayCommand<RoleDto> DeactivateCommand { get; }

    public RolesViewModel(
        IRoleApi roleApi,
        IToastService toastService,
        IDialogService dialogService,
        ILogger<RolesViewModel> logger)
    {
        _roleApi = roleApi;
        _toastService = toastService;
        _dialogService = dialogService;
        _logger = logger;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        NextPageCommand = new AsyncRelayCommand(OnNextPage);
        PreviousPageCommand = new AsyncRelayCommand(OnPreviousPage);
        RefreshCommand = new AsyncRelayCommand(OnRefresh);
        SearchCommand = new AsyncRelayCommand<string>(OnSearch);
        DeleteCommand = new AsyncRelayCommand<RoleDto>(DeleteRole);
        ActivateCommand = new AsyncRelayCommand<RoleDto>(ActivateRole);
        DeactivateCommand = new AsyncRelayCommand<RoleDto>(DeactivateRole);
    }

    private async Task LoadDataAsync()
    {
        if (IsListLoading) return;

        try
        {
            IsListLoading = true;
            HasNoData = false;

            var result = await _roleApi.SearchRoles(Page, PageSize, SearchTerm, SortColumn, SortDirection);

            Roles.Clear();
            foreach (var item in result.Items)
            {
                Roles.Add(item);
            }

            TotalCount = result.TotalCount;
            HasNoData = TotalCount == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading roles");
            HasNoData = true;
        }
        finally
        {
            IsListLoading = false;
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

    public async Task ActivateRole(RoleDto? role)
    {
        if (role == null || IsBusy) return;
        try
        {
            IsBusy = true;
            await _roleApi.ActivateRole(role.Id);
            _toastService.ShowSuccess("Success", "Role activated successfully.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating role");
            _toastService.ShowError("Error", "Failed to activate role.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DeactivateRole(RoleDto? role)
    {
        if (role == null || IsBusy) return;
        try
        {
            IsBusy = true;
            await _roleApi.DeactivateRole(role.Id);
            _toastService.ShowSuccess("Success", "Role deactivated successfully.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating role");
            _toastService.ShowError("Error", "Failed to deactivate role.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DeleteRole(RoleDto? role)
    {
        if (role == null || IsBusy) return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Delete Role",
            $"Are you sure you want to delete the role '{role.Name}'?",
            "Delete",
            "Cancel");

        if (confirmed)
        {
            try
            {
                IsBusy = true;
                await _roleApi.DeleteRole(role.Id);
                _toastService.ShowSuccess("Success", "Role deleted successfully.");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role");
                _toastService.ShowError("Error", "Failed to delete role.");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadDataAsync();
    }

    public void OnNavigatedFrom()
    {
    }
}
