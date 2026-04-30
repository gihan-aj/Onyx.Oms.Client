using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class EditOrderViewModel : ObservableObject, INavigationAware
    {
        private readonly IOrdersApi _ordersApi;
        private readonly ILogger<EditOrderViewModel> _logger;
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastService;
        private readonly IFileService _fileService;

        private Guid? _orderId;

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

        // Logistics
        private OrderLogisticsViewModel? _logistics = null;
        public OrderLogisticsViewModel? Logistics
        {
            get => _logistics;
            set => SetProperty(ref _logistics, value);
        }

        public bool HasCustomerDetails => CustomerDetails != null;

        // Order items
        private OrderItemsViewModel? _orderItems = null;
        public OrderItemsViewModel? OrderItems
        {
            get => _orderItems;
            set => SetProperty(ref _orderItems, value);
        }

        public IRelayCommand GoBackCommand { get; }
        public IAsyncRelayCommand UpdateOrderLogisticsCommand { get; }
        public IAsyncRelayCommand UpdateOrderItemsCommand { get; }

        public EditOrderViewModel(IOrdersApi ordersApi, ILogger<EditOrderViewModel> logger, INavigationService navigationService, IToastService toastService, IFileService fileService)
        {
            _ordersApi = ordersApi;
            _logger = logger;
            _navigationService = navigationService;
            _toastService = toastService;
            _fileService = fileService;

            GoBackCommand = new RelayCommand(GoBack);
            UpdateOrderLogisticsCommand = new AsyncRelayCommand(UpdateLogisticsAsync);
            UpdateOrderItemsCommand = new AsyncRelayCommand(UpdateOrderItemsAsync);
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
                _orderId = orderDetails.Id;
                PageTitle = orderDetails.OrderNumber;
                CustomerDetails = new CustomerDetailsViewModel(orderDetails.Customer);
                Logistics = new OrderLogisticsViewModel(orderDetails, _toastService);
                await LoadCouriersAsync(Logistics);
                OrderItems = new OrderItemsViewModel(orderDetails, _fileService, _toastService);
                await OrderItems.LoadImagesAsync();
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

        private async Task LoadCouriersAsync(OrderLogisticsViewModel logistics)
        {
            try
            {
                var couriers = await _ordersApi.GetCouriers(true);
                logistics.Couriers.Clear();
                foreach (var c in couriers)
                {
                    logistics.Couriers.Add(c);
                }
                if (logistics.CourierId.HasValue)
                {
                    var selected = logistics.Couriers.FirstOrDefault(c => c.Id == logistics.CourierId.Value);
                    if(selected != null)
                        logistics.SelectedCourier = selected;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load couriers at order edit page.");
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

        private async Task UpdateLogisticsAsync()
        {
            if (Logistics == null || !_orderId.HasValue)
                return;

            var request = Logistics.GetUpdateDto();
            if (request == null)
                return;

            try
            {
                IsBusy = true;
                await _ordersApi.UpdateLogistics(_orderId.Value, request);
                await InitializeAsync(_orderId.Value);
                _toastService.ShowSuccess("Success", "Order logistics has been updated.");
            }
            catch
            {
                _logger.LogError("Failed to update order logistics details for order ID: {OrderId}", _orderId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateOrderItemsAsync()
        {
            if (OrderItems == null || !_orderId.HasValue)
                return;

            var request = OrderItems.GetUpdateDto();
            if (request == null)
                return;

            try
            {
                IsBusy = true;
                await _ordersApi.UpdateFinancials(_orderId.Value, request);
                await InitializeAsync(_orderId.Value);
                _toastService.ShowSuccess("Success", "Order items has been updated.");
            }
            catch
            {
                _logger.LogError("Failed to update order items for order ID: {OrderId}", _orderId);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
