using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly IFileService _fileService;

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
                    if (UseCustomerAddress)
                    {
                        AutoFillAddress();
                    }
                }
            }
        }

        private CreateCustomerCommand? _draftCustomer = null;

        // Order items
        public ObservableCollection<CreateOrderLineItem> OrderItems { get; } = new();

        // Notes
        private string? _notes;
        public string? Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        // Logistics State
        public ObservableCollection<CourierDto> Couriers { get; } = new();

        private CourierDto? _selectedCourier;
        public CourierDto? SelectedCourier { get => _selectedCourier; set => SetProperty(ref _selectedCourier, value); }

        private bool _isCashOnDelivery = true;
        public bool IsCashOnDelivery { get => _isCashOnDelivery; set => SetProperty(ref _isCashOnDelivery, value); }

        private bool _useCustomerAddress;
        public bool UseCustomerAddress
        {
            get => _useCustomerAddress;
            set
            {
                if (SetProperty(ref _useCustomerAddress, value))
                {
                    if (value)
                    {
                        AutoFillAddress();
                    }
                }
            }
        }

        // Shipping Address
        private string _shippingStreet = string.Empty;
        public string ShippingStreet { get => _shippingStreet; set => SetProperty(ref _shippingStreet, value); }

        private string _shippingCity = string.Empty;
        public string ShippingCity { get => _shippingCity; set => SetProperty(ref _shippingCity, value); }

        private string _shippingState = string.Empty;
        public string ShippingState 
        { 
            get => _shippingState; 
            set 
            {
                if (SetProperty(ref _shippingState, value))
                {
                    UpdateDistricts(value);
                }
            } 
        }

        private string _shippingDistrict = string.Empty;
        public string ShippingDistrict { get => _shippingDistrict; set => SetProperty(ref _shippingDistrict, value); }

        private string _shippingPostalCode = string.Empty;
        public string ShippingPostalCode { get => _shippingPostalCode; set => SetProperty(ref _shippingPostalCode, value); }

        private string _shippingCountry = string.Empty;
        public string ShippingCountry { get => _shippingCountry; set => SetProperty(ref _shippingCountry, value); }

        private string[] _districts = Array.Empty<string>();
        public string[] Districts 
        { 
            get => _districts; 
            private set => SetProperty(ref _districts, value);
        }

        public IReadOnlyList<string> Provinces { get; } = new[]
        {
            "Central", "Eastern", "North Central", "Northern", "North Western", "Sabaragamuwa", "Southern", "Uva", "Western"
        };

        private readonly Dictionary<string, string[]> _districtsByProvince = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Central", new[] { "Kandy", "Matale", "Nuwara Eliya" } },
            { "Eastern", new[] { "Ampara", "Batticaloa", "Trincomalee" } },
            { "North Central", new[] { "Anuradhapura", "Polonnaruwa" } },
            { "Northern", new[] { "Jaffna", "Kilinochchi", "Mannar", "Mullaitivu", "Vavuniya" } },
            { "North Western", new[] { "Kurunegala", "Puttalam" } },
            { "Sabaragamuwa", new[] { "Kegalle", "Ratnapura" } },
            { "Southern", new[] { "Galle", "Hambantota", "Matara" } },
            { "Uva", new[] { "Badulla", "Monaragala" } },
            { "Western", new[] { "Colombo", "Gampaha", "Kalutara" } }
        };

        private void UpdateDistricts(string province)
        {
            if (string.IsNullOrWhiteSpace(province) || !_districtsByProvince.TryGetValue(province, out var districts))
            {
                Districts = Array.Empty<string>();
            }
            else
            {
                Districts = districts;
            }

            if (!string.IsNullOrWhiteSpace(ShippingDistrict) && Array.IndexOf(Districts, ShippingDistrict) == -1)
            {
                ShippingDistrict = string.Empty;
            }
        }

        private void AutoFillAddress()
        {
            if (SelectedCustomer?.Address != null)
            {
                ShippingStreet = SelectedCustomer.Address.Street ?? string.Empty;
                ShippingCity = SelectedCustomer.Address.City ?? string.Empty;
                ShippingState = SelectedCustomer.Address.State ?? string.Empty;
                ShippingDistrict = SelectedCustomer.Address.District ?? string.Empty;
                ShippingPostalCode = SelectedCustomer.Address.PostalCode ?? string.Empty;
                ShippingCountry = SelectedCustomer.Address.Country ?? string.Empty;
            }
        }

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
        public IRelayCommand<CreateOrderLineItem> RemoveLineItemCommand { get; }

        public CreateOrderViewModel(
            IOrdersApi ordersApi,
            ILogger<CreateOrderViewModel> logger,
            IToastService toastService,
            INavigationService navigationService,
            ITenantProfileService tenantProfileService,
            IDialogService dialogService,
            IFileService fileService)
        {
            _ordersApi = ordersApi;
            _logger = logger;
            _toastService = toastService;
            _navigationService = navigationService;
            _tenantProfileService = tenantProfileService;
            _dialogService = dialogService;
            _fileService = fileService;

            SaveCommand = new AsyncRelayCommand(OnSaveExecuteAsync);
            CancelCommand = new RelayCommand(OnCancelExecute);
            CreateNewCustomerCommand = new AsyncRelayCommand(OnCreateNewCustomerAsync);
            ClearCustomerCommand = new RelayCommand(() => SelectedCustomer = null);
            ShowProductPickerCommand = new AsyncRelayCommand(OnShowProductPickerExecuteAsync);
            RemoveLineItemCommand = new RelayCommand<CreateOrderLineItem>(OnRemoveLineItem);
        }

        public void OnNavigatedFrom()
        {

        }

        public async void OnNavigatedTo(object parameter)
        {
            BaseCurrency = _tenantProfileService.Profile?.BaseCurrency ?? "LKR";
            await LoadCouriersAsync();
        }

        private async Task LoadCouriersAsync()
        {
            try
            {
                var couriers = await _ordersApi.GetCouriers(true);
                Couriers.Clear();
                foreach (var c in couriers)
                {
                    Couriers.Add(c);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load couriers.");
            }
        }

        private void OnCancelExecute()
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
        }

        private void OnRemoveLineItem(CreateOrderLineItem? item)
        {
            if (item != null && OrderItems.Contains(item))
            {
                OrderItems.Remove(item);
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

            dialog.ViewModel.OnProductAdded = async (gridItem, qty) => 
            {
                decimal price = gridItem.ResolvedVariant != null && gridItem.ResolvedVariant.PriceAmount > 0 
                    ? gridItem.ResolvedVariant.PriceAmount 
                    : gridItem.BasePriceAmount;

                var lineItem = new CreateOrderLineItem(_fileService)
                {
                    ProductId = gridItem.Id,
                    ProductVariantId = gridItem.ResolvedVariant?.Id,
                    ProductName = gridItem.DisplayName,
                    Sku = gridItem.DisplaySku,
                    ImageUrl = gridItem.ResolvedImageUrl,
                    BaseCurrency = gridItem.BasePriceCurrency,
                    UnitPrice = price,
                    Quantity = qty,
                    AvailableQuantity = gridItem.ResolvedVariant != null 
                        ? (gridItem.ResolvedVariant.StockOnHand - gridItem.ResolvedVariant.ReservedQuantity) 
                        : gridItem.AvailableQuantity
                };

                var existingItem = OrderItems.FirstOrDefault(i => i.ProductId == lineItem.ProductId && i.ProductVariantId == lineItem.ProductVariantId);
                if (existingItem != null)
                {
                    existingItem.Quantity += lineItem.Quantity;
                }
                else
                {
                    await lineItem.LoadImageAsync();
                    OrderItems.Add(lineItem);
                }

                _toastService.ShowSuccess("Item Added", $"Added {qty}x {lineItem.ProductName} to the order.");
            };

            await dialog.ShowAsync();
        }
    }
}
