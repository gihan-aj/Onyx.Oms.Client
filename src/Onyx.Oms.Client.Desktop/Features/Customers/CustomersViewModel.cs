using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Customers;

public partial class CustomersViewModel : ObservableObject, INavigationAware
{
    private readonly ICustomerApi _customerApi;
    private readonly IToastService _toastService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<CustomersViewModel> _logger;
    private readonly INavigationService _navigationService;
    private readonly IPermissionService _permissionService;

    private ObservableCollection<CustomerDto> _items = new();
    public ObservableCollection<CustomerDto> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
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
    public IAsyncRelayCommand<CustomerDto> DeleteCommand { get; }
    public IAsyncRelayCommand<CustomerDto> ActivateCommand { get; }
    public IAsyncRelayCommand<CustomerDto> DeactivateCommand { get; }
    public IRelayCommand NewCustomerCommand { get; }
    public IRelayCommand<CustomerDto> EditCustomerCommand { get; }

    public CustomersViewModel(
        ICustomerApi customerApi,
        IToastService toastService,
        IDialogService dialogService,
        ILogger<CustomersViewModel> logger,
        INavigationService navigationService,
        IPermissionService permissionService)
    {
        _customerApi = customerApi;
        _toastService = toastService;
        _dialogService = dialogService;
        _logger = logger;
        _navigationService = navigationService;
        _permissionService = permissionService;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        NextPageCommand = new AsyncRelayCommand(OnNextPage);
        PreviousPageCommand = new AsyncRelayCommand(OnPreviousPage);
        RefreshCommand = new AsyncRelayCommand(OnRefresh);
        SearchCommand = new AsyncRelayCommand<string>(OnSearch);
        DeleteCommand = new AsyncRelayCommand<CustomerDto>(DeleteCustomer);
        ActivateCommand = new AsyncRelayCommand<CustomerDto>(ActivateCustomer);
        DeactivateCommand = new AsyncRelayCommand<CustomerDto>(DeactivateCustomer);
        NewCustomerCommand = new RelayCommand(OnNewCustomer);
        EditCustomerCommand = new RelayCommand<CustomerDto>(OnEditCustomer);
    }

    private void OnNewCustomer()
    {
        _navigationService.NavigateTo("Onyx.Oms.Client.Desktop.Features.Customers.CustomerFormPage");
    }

    private void OnEditCustomer(CustomerDto? customer)
    {
        if (customer != null)
        {
            _navigationService.NavigateTo("Onyx.Oms.Client.Desktop.Features.Customers.CustomerFormPage", customer.Id);
        }
    }

    public async Task<CustomerDto?> GetCustomerDetailsAsync(Guid customerId)
    {
        try
        {
            return await _customerApi.GetCustomerById(customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching customer details for ID {CustomerId}", customerId);
            return null;
        }
    }

    private async Task LoadDataAsync()
    {
        if (IsListLoading) return;

        try
        {
            IsListLoading = true;
            HasNoData = false;

            var result = await _customerApi.SearchCustomers(Page, PageSize, SearchTerm, SortColumn, SortDirection);
            
            // Evaluate permissions once
            var canEdit = _permissionService.CanExecute(Shared.Constants.Permissions.Customers.Edit);
            var canDelete = _permissionService.CanExecute(Shared.Constants.Permissions.Customers.Delete);
            var canActivate = _permissionService.CanExecute(Shared.Constants.Permissions.Customers.Activate);
            var canDeactivate = _permissionService.CanExecute(Shared.Constants.Permissions.Customers.Deactivate);

            Items.Clear();
            foreach (var item in result.Items)
            {
                item.CanEdit = canEdit;
                item.CanDelete = canDelete;
                item.CanActivate = canActivate;
                item.CanDeactivate = canDeactivate;
                
                Items.Add(item);
            }

            TotalCount = result.TotalCount;
            HasNoData = TotalCount == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customers");
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
        SearchTerm = null;
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

    public async Task ActivateCustomer(CustomerDto? customer)
    {
        if (customer == null || IsBusy) return;
        try
        {
            IsBusy = true;
            await _customerApi.ActivateCustomer(customer.Id);
            _toastService.ShowSuccess("Success", "Customer activated successfully.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating customer");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DeactivateCustomer(CustomerDto? customer)
    {
        if (customer == null || IsBusy) return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Deactivate Customer",
            $"Are you sure you want to deactivate the customer '{customer.Name}'?",
            "Deactivate",
            "Cancel");

        if (confirmed)
        {
            try
            {
                IsBusy = true;
                await _customerApi.DeactivateCustomer(customer.Id);
                _toastService.ShowSuccess("Success", "Customer deactivated successfully.");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating customer");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public async Task DeleteCustomer(CustomerDto? customer)
    {
        if (customer == null || IsBusy) return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Delete Customer",
            $"Are you sure you want to delete the customer '{customer.Name}'?",
            "Delete",
            "Cancel");

        if (confirmed)
        {
            try
            {
                IsBusy = true;
                await _customerApi.DeleteCustomer(customer.Id);
                _toastService.ShowSuccess("Success", "Customer deleted successfully.");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer");
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
