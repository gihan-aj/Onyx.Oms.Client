using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class EditOrderViewModel : ObservableObject, INavigationAware
    {
        private readonly IOrdersApi _ordersApi;
        private readonly ILogger<EditOrderViewModel> _logger;
        private readonly INavigationService _navigationService;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _pageTitle = string.Empty;
        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        // Customer
        private CustomerDetailsViewModel? _customerDetails = null;
        public CustomerDetailsViewModel? CustomerDetails
        {
            get => _customerDetails;
            set
            {
                if (SetProperty(ref _customerDetails, value))
                {
                    OnPropertyChanged(nameof(HasCustomerDetails));
                }
            }
        }

        public bool HasCustomerDetails => CustomerDetails != null;

        public IRelayCommand GoBackCommand { get; }

        public EditOrderViewModel(IOrdersApi ordersApi, ILogger<EditOrderViewModel> logger, INavigationService navigationService)
        {
            _ordersApi = ordersApi;
            _logger = logger;
            _navigationService = navigationService;

            GoBackCommand = new RelayCommand(GoBack);
        }

        public void OnNavigatedFrom()
        {
            
        }

        public async void OnNavigatedTo(object parameter)
        {
            if(parameter is Guid orderId)
            {
               await InitializeAsync(orderId);
            }
            else if (parameter is string orderIdString && Guid.TryParse(orderIdString, out var parsedId))
            {
                await InitializeAsync(parsedId);
            }
        }

        private async Task InitializeAsync(Guid orderId)
        {
            try
            {
                IsLoading = true;
                var orderDetails = await _ordersApi.GetOrderById(orderId);
                PageTitle = orderDetails.OrderNumber;
                CustomerDetails = new CustomerDetailsViewModel(orderDetails.Customer);
            }
            catch
            {
                _logger.LogError("Failed to load order details for order ID: {OrderId}", orderId);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GoBack()
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
            else
            {
                _navigationService.NavigateTo("Onyx.Oms.Client.Desktop.Features.Orders.List.OrdersPage");
            }
        }
    }
}
