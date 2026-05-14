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
                    OnPropertyChanged(nameof(HasSecondaryPhone));
                    OnPropertyChanged(nameof(HasEmail));
                    OnPropertyChanged(nameof(HasNotes));
                    OnPropertyChanged(nameof(HasAddress));
                    if(value != null && !string.IsNullOrWhiteSpace(value.DeliveryInstructions))
                    {
                        DeliveryInstructions = value.DeliveryInstructions;
                    }
                }
            }
        }

        public bool HasSecondaryPhone => string.IsNullOrWhiteSpace(SelectedCustomer?.SecondaryPhone) ? false : true;
        public bool HasEmail => string.IsNullOrWhiteSpace(SelectedCustomer?.Email) ? false : true;
        public bool HasNotes => string.IsNullOrWhiteSpace(SelectedCustomer?.Notes) ? false : true;
        public bool HasAddress => SelectedCustomer?.Address != null && (!string.IsNullOrWhiteSpace(SelectedCustomer.Address.Street));

        private CreateCustomerCommand? _draftCustomer = null;

        // Order Date & Time
        private DateTimeOffset _orderDate = DateTimeOffset.Now;
        public DateTimeOffset OrderDate
        {
            get => _orderDate;
            set => SetProperty(ref _orderDate, value);
        }
        private TimeSpan _orderTime = DateTime.Now.TimeOfDay;
        public TimeSpan OrderTime
        {
            get => _orderTime;
            set => SetProperty(ref _orderTime, value);
        }

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

        private string? _deliveryInstructions;
        public string? DeliveryInstructions
        {
            get => _deliveryInstructions;
            set => SetProperty(ref _deliveryInstructions, value);
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

        // Financial Adjustments
        private decimal _shippingFee;
        public decimal ShippingFee
        {
            get => _shippingFee;
            set
            {
                if (SetProperty(ref _shippingFee, value))
                {
                    RecalculateTotals();
                }
            }
        }

        private decimal _taxAmount;
        public decimal TaxAmount
        {
            get => _taxAmount;
            set
            {
                if (SetProperty(ref _taxAmount, value))
                {
                    RecalculateTotals();
                }
            }
        }

        private OrderDiscountDto? _appliedDiscount;
        public OrderDiscountDto? AppliedDiscount
        {
            get => _appliedDiscount;
            set
            {
                if (SetProperty(ref _appliedDiscount, value))
                {
                    OnPropertyChanged(nameof(HasDiscount));
                    RecalculateTotals();
                }
            }
        }

        public bool HasDiscount => AppliedDiscount != null;

        // Financial Summary
        private decimal _subTotal;
        public decimal SubTotal
        {
            get => _subTotal;
            private set => SetProperty(ref _subTotal, value);
        }

        private decimal _discountAmount;
        public decimal DiscountAmount
        {
            get => _discountAmount;
            private set => SetProperty(ref _discountAmount, value);
        }

        private decimal _grandTotal;
        public decimal GrandTotal
        {
            get => _grandTotal;
            private set => SetProperty(ref _grandTotal, value);
        }

        public string FormatCurrency(decimal value)
        {
            return value.ToString("N2");
        }

        private void RecalculateTotals()
        {
            decimal subTotal = 0;
            foreach (var item in OrderItems)
            {
                subTotal += item.LineTotal;
            }
            SubTotal = subTotal;

            decimal discount = 0;
            if (AppliedDiscount != null)
            {
                if (AppliedDiscount.Type == DiscountType.FlatAmount)
                {
                    discount = Math.Round(AppliedDiscount.Value, 0, MidpointRounding.AwayFromZero);
                }
                else if (AppliedDiscount.Type == DiscountType.Percentage)
                {
                   var calculated = SubTotal * (AppliedDiscount.Value / 100m);
                   discount = Math.Round(calculated, 0, MidpointRounding.AwayFromZero);
                }
            }
            DiscountAmount = discount;

            GrandTotal = SubTotal + ShippingFee + TaxAmount - DiscountAmount;
        }

        // Payment
        private decimal _paymentAmount;
        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set => SetProperty(ref _paymentAmount, value);
        }

        private PaymentMethod _paymentMethod = PaymentMethod.BankTransfer;
        public PaymentMethod PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        public List<PaymentMethod> PaymentMethods { get; } = new() 
        { 
            PaymentMethod.BankTransfer, 
            PaymentMethod.Card
        };

        private string _paymentReference = string.Empty;
        public string PaymentReference
        {
            get => _paymentReference;
            set => SetProperty(ref _paymentReference, value);
        }

        private DateTimeOffset _paymentDate = DateTimeOffset.Now;
        public DateTimeOffset PaymentDate
        {
            get => _paymentDate;
            set => SetProperty(ref _paymentDate, value);
        }

        private TimeSpan _paymentTime = DateTime.Now.TimeOfDay;
        public TimeSpan PaymentTime
        {
            get => _paymentTime;
            set => SetProperty(ref _paymentTime, value);
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
        public IAsyncRelayCommand ShowApplyDiscountDialogCommand { get; }
        public IRelayCommand ClearDiscountCommand { get; }
        public IRelayCommand PayInFullCommand { get; }
        public IAsyncRelayCommand ShowCustomerOrderHistoryCommand { get; }

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
            ShowApplyDiscountDialogCommand = new AsyncRelayCommand(OnShowApplyDiscountDialogAsync);
            ClearDiscountCommand = new RelayCommand(() => AppliedDiscount = null);
            PayInFullCommand = new RelayCommand(() => PaymentAmount = GrandTotal);
            ShowCustomerOrderHistoryCommand = new AsyncRelayCommand(ShowCustomerOrderHistoryAsync);

            OrderItems.CollectionChanged += OrderItems_CollectionChanged;
        }

        private void OrderItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CreateOrderLineItem item in e.NewItems)
                {
                    item.PropertyChanged += LineItem_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (CreateOrderLineItem item in e.OldItems)
                {
                    item.PropertyChanged -= LineItem_PropertyChanged;
                }
            }
            RecalculateTotals();
        }

        private void LineItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CreateOrderLineItem.LineTotal))
            {
                RecalculateTotals();
            }
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

        private async Task ShowCustomerOrderHistoryAsync()
        {
            if (SelectedCustomer == null)
                return;

            try
            {
                IsBusy = true;
                var orderHistory = await _ordersApi.GetCustomerOrderHistory(SelectedCustomer.Id);
                var dialog = new CustomerOrderHistoryDialog(orderHistory)
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                IsBusy = false;

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to show customer order history at order edit page.");
            }
        }

        private async Task OnSaveExecuteAsync()
        {
            if (SelectedCustomer == null)
            {
                _toastService.ShowError("Validation Error", "Please select or create a customer.");
                return;
            }

            if (OrderItems.Count == 0)
            {
                _toastService.ShowError("Validation Error", "Please add at least one item to the order.");
                return;
            }

            try
            {
                IsBusy = true;

                var items = OrderItems.Select(i => new OrderItemDto(
                    Id: null,
                    ProductVariantId: i.ProductVariantId ?? i.ProductId,
                    Quantity: i.Quantity,
                    Discount: null
                )).ToList();

                ShippingAddressDto? shippingAddress = null;
                if (!string.IsNullOrWhiteSpace(ShippingStreet) || !string.IsNullOrWhiteSpace(ShippingCity))
                {
                    shippingAddress = new ShippingAddressDto(
                        Street: string.IsNullOrWhiteSpace(ShippingStreet) ? null : ShippingStreet,
                        City: string.IsNullOrWhiteSpace(ShippingCity) ? null : ShippingCity,
                        District: string.IsNullOrWhiteSpace(ShippingDistrict) ? null : ShippingDistrict,
                        State: string.IsNullOrWhiteSpace(ShippingState) ? null : ShippingState,
                        PostalCode: string.IsNullOrWhiteSpace(ShippingPostalCode) ? null : ShippingPostalCode,
                        Country: string.IsNullOrWhiteSpace(ShippingCountry) ? null : ShippingCountry
                    );
                }

                InitialPaymentDto? initialPayment = null;
                if (!IsCashOnDelivery && PaymentAmount > 0)
                {
                    initialPayment = new InitialPaymentDto(
                        Amount: new MoneyDto(PaymentAmount, BaseCurrency),
                        Method: PaymentMethod,
                        Reference: string.IsNullOrWhiteSpace(PaymentReference) ? null : PaymentReference,
                        PaymentDate: PaymentDate.Date + PaymentTime
                    );
                }

                var command = new CreateOrderCommand(
                    CustomerId: SelectedCustomer.Id,
                    IsCashOnDelivery: IsCashOnDelivery,
                    OrderDate: OrderDate.Date + OrderTime,
                    Items: items,
                    CourierId: SelectedCourier?.Id,
                    ShippingAddress: shippingAddress,
                    ShippingFee: ShippingFee > 0 ? new MoneyDto(ShippingFee, BaseCurrency) : null,
                    TaxAmount: TaxAmount > 0 ? new MoneyDto(TaxAmount, BaseCurrency) : null,
                    Discount: AppliedDiscount,
                    Payment: initialPayment,
                    Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                    DeliveryInstructions: string.IsNullOrWhiteSpace(DeliveryInstructions) ? null : DeliveryInstructions
                );

                var orderId = await _ordersApi.CreateOrder(command);
                _toastService.ShowSuccess("Success", "Order created successfully!");

                if (_navigationService.CanGoBack)
                {
                    _navigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order.");
                //_toastService.ShowError("Error", "Failed to create order. Please try again.");
            }
            finally
            {
                IsBusy = false;
            }
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
                    DeliveryInstructions = string.IsNullOrWhiteSpace(dialog.DeliveryInstructions) ? null : dialog.DeliveryInstructions,
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

        private async Task OnShowApplyDiscountDialogAsync()
        {
            var dialog = new ApplyDiscountDialog();
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;

            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && dialog.Result != null)
            {
                AppliedDiscount = dialog.Result;
            }
        }
    }
}
