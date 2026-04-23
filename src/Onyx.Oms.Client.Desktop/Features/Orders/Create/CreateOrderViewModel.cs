using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Create
{
    public partial class CreateOrderViewModel : ObservableObject, INavigationAware
    {
        private readonly IOrdersApi _ordersApi;
        private readonly ILogger<CreateOrderViewModel> _logger;
        private readonly IToastService _toastService;
        private readonly INavigationService _navigationService;
        //private readonly IFileService _fileService;
        private readonly ITenantProfileService _tenantProfileService;

        public string Title => "Create Order";

        private string _baseCurrency = "LKR";
        public string BaseCurrency { get => _baseCurrency; set => SetProperty(ref _baseCurrency, value); }

        // --- UI State ---
        private bool _isLoading = true;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        private bool _isBusy = false;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public CreateOrderViewModel(
            IOrdersApi ordersApi,
            ILogger<CreateOrderViewModel> logger,
            IToastService toastService,
            INavigationService navigationService,
            ITenantProfileService tenantProfileService)
        {
            _ordersApi = ordersApi;
            _logger = logger;
            _toastService = toastService;
            _navigationService = navigationService;
            _tenantProfileService = tenantProfileService;

            SaveCommand = new AsyncRelayCommand(OnSaveExecuteAsync);
            CancelCommand = new RelayCommand(OnCancelExecute);
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
    }
}
