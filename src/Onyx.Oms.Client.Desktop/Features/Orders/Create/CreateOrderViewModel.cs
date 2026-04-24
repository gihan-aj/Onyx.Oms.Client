using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Create
{
    public partial class CreateOrderViewModel : ObservableObject, INavigationAware
    {
        private readonly IOrdersApi _ordersApi;
        private readonly ILogger<CreateOrderViewModel> _logger;
        private readonly IToastService _toastService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        //private readonly IFileService _fileService;
        private readonly ITenantProfileService _tenantProfileService;

        public string Title => "Create Order";

        private string _baseCurrency = "LKR";
        public string BaseCurrency { get => _baseCurrency; set => SetProperty(ref _baseCurrency, value); }

        // Customer details
        public bool HasSelectedCustomer => SelectedCustomer != null;

        private CustomerDto? _selectedCustomer;
        public CustomerDto? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    OnPropertyChanged(nameof(HasSelectedCustomer));
                }
            }
        }

        private CreateCustomerCommand? _draftCustomer = null;

        // --- UI State ---
        private bool _isLoading = true;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        private bool _isBusy = false;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand CreateNewCustomerCommand { get; }
        public IRelayCommand ClearCustomerCommand { get; }
        public IAsyncRelayCommand ShowProductPickerCommand { get; }

        public CreateOrderViewModel(
            IOrdersApi ordersApi,
            ILogger<CreateOrderViewModel> logger,
            IToastService toastService,
            INavigationService navigationService,
            ITenantProfileService tenantProfileService,
            IDialogService dialogService)
        {
            _ordersApi = ordersApi;
            _logger = logger;
            _toastService = toastService;
            _navigationService = navigationService;
            _tenantProfileService = tenantProfileService;
            _dialogService = dialogService;

            SaveCommand = new AsyncRelayCommand(OnSaveExecuteAsync);
            CancelCommand = new RelayCommand(OnCancelExecute);
            CreateNewCustomerCommand = new AsyncRelayCommand(OnCreateNewCustomerAsync);
            ClearCustomerCommand = new RelayCommand(() => SelectedCustomer = null);
            ShowProductPickerCommand = new AsyncRelayCommand(OnShowProductPickerExecuteAsync);
        }

        public void OnNavigatedFrom()
        {

        }

        public void OnNavigatedTo(object parameter)
        {
            BaseCurrency = _tenantProfileService.Profile?.BaseCurrency ?? "LKR";
        }

        private void OnCancelExecute()
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
        }

        private async Task OnSaveExecuteAsync()
        {

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

        private async Task OnCreateNewCustomerAsync()
        {
            var dialog = new CreateNewCustomerDialog(_draftCustomer) { XamlRoot = _dialogService.CurrentXamlRoot };
            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                var command = new CreateCustomerCommand
                {
                    Name = dialog.CustomerName,
                    Email = dialog.Email,
                    PrimaryPhone = dialog.PrimaryPhone,
                    SecondaryPhone = string.IsNullOrWhiteSpace(dialog.SecondaryPhone) ? null : dialog.SecondaryPhone,
                    Street = dialog.Street,
                    City = dialog.City,
                    State = dialog.State,
                    District = dialog.District,
                    PostalCode = dialog.PostalCode,
                    Country = dialog.Country,
                    Notes = string.IsNullOrWhiteSpace(dialog.Notes) ? null : dialog.Notes
                };

                _draftCustomer = command;

                try
                {
                    IsBusy = true;
                    var customerId = await _ordersApi.CreateCustomer(command);
                    SelectedCustomer = await _ordersApi.GetCustomerById(customerId);
                    _toastService.ShowSuccess("Success", "Customer created successfully.");
                    _draftCustomer = null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating customer");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task OnShowProductPickerExecuteAsync()
        {
            var dialog = new ProductPicker.ProductPicker();
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;

            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && dialog.ViewModel.SelectedItem != null)
            {
                
            }
        }
    }
}
