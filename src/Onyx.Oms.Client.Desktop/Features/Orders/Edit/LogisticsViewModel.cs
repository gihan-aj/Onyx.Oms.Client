using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class OrderLogisticsViewModel : ObservableObject
    {
        private readonly IToastService _toastService;

        private UpdateOrderLogisticsCommand _originalLogistics;
        private readonly OrderStatus _orderStatus;
        private readonly AddressDto? _customerAddress;
        private readonly string? _customerDeliveryInstructions;

        private bool _canEdit;
        public bool CanEdit
        {
            get => _canEdit;
            set => SetProperty(ref _canEdit, value);
        }

        private bool _isEditing = false;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    OnPropertyChanged(nameof(IsReadonly));
                    CanEdit = !value && _orderStatus < OrderStatus.Shipped;
                }
            }
        }

        public bool IsReadonly => !IsEditing;

        private bool _isCashOnDelivery;
        public bool IsCashOnDelivery
        {
            get => _isCashOnDelivery;
            set => SetProperty(ref _isCashOnDelivery, value);
        }

        private Guid? _courierId;
        public Guid? CourierId
        {
            get => _courierId;
            set => SetProperty(ref _courierId, value);
        }

        public ObservableCollection<CourierDto> Couriers { get; } = new();

        private CourierDto? _selectedCourier;
        public CourierDto? SelectedCourier
        {
            get => _selectedCourier;
            set
            {
                if (!IsEditing && value == null) return;
                if (SetProperty(ref _selectedCourier, value))
                {
                    if (value != null)
                        CourierId = value.Id;
                    else CourierId = null;

                    if (!string.IsNullOrWhiteSpace(ShippingAddressDistrict))
                    {
                        OnCourierSelected?.Invoke();
                    }
                }
            }
        }

        public Func<Task>? OnCourierSelected { get; set; }

        // Fields for the Shipping Address
        private string? _shippingAddressStreet;
        public string? ShippingAddressStreet
        {
            get => _shippingAddressStreet;
            set
            {
                if (SetProperty(ref _shippingAddressStreet, value))
                    OnPropertyChanged(nameof(HasShippingAddress));
            }
        }

        private string? _shippingAddressCity;
        public string? ShippingAddressCity
        {
            get => _shippingAddressCity;
            set
            {
                if (SetProperty(ref _shippingAddressCity, value))
                    OnPropertyChanged(nameof(HasShippingAddress));
            }
        }

        private string? _shippingAddressDistrict;
        public string? ShippingAddressDistrict
        {
            get => _shippingAddressDistrict;
            set
            {
                if (!IsEditing && value == null) return;
                if (SetProperty(ref _shippingAddressDistrict, value))
                {
                    if (SelectedCourier != null)
                        OnCourierSelected?.Invoke();
                }
            }
        }

        private string? _shippingAddressState;
        public string? ShippingAddressState
        {
            get => _shippingAddressState;
            set
            {
                if (!IsEditing && value == null) return;
                if (SetProperty(ref _shippingAddressState, value))
                {
                    UpdateDistricts(value);
                }
            }
        }

        private string? _shippingAddressPostalCode;
        public string? ShippingAddressPostalCode
        {
            get => _shippingAddressPostalCode;
            set => SetProperty(ref _shippingAddressPostalCode, value);
        }

        private string? _shippingAddressCountry;
        public string? ShippingAddressCountry
        {
            get => _shippingAddressCountry;
            set => SetProperty(ref _shippingAddressCountry, value);
        }

        private string? _trackingNumber;
        public string? TrackingNumber
        {
            get => _trackingNumber;
            set
            {
                if (SetProperty(ref _trackingNumber, value))
                    OnPropertyChanged(nameof(HasTrackingNumber));
            }
        }

        public bool HasTrackingNumber => !string.IsNullOrWhiteSpace(TrackingNumber);
        public bool HasShippingAddress => !string.IsNullOrWhiteSpace(ShippingAddressStreet) || !string.IsNullOrWhiteSpace(ShippingAddressCity);

        private string[] _districts = Array.Empty<string>();
        public string[] Districts
        {
            get => _districts;
            private set => SetProperty(ref _districts, value);
        }

        public ObservableCollection<string> Provinces { get; } = new()
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

        private void UpdateDistricts(string? province)
        {
            if (string.IsNullOrWhiteSpace(province) || !_districtsByProvince.TryGetValue(province, out var districts))
            {
                Districts = Array.Empty<string>();
            }
            else
            {
                Districts = districts;
            }

            if (!string.IsNullOrWhiteSpace(ShippingAddressDistrict) && Array.IndexOf(Districts, ShippingAddressDistrict) == -1)
            {
                ShippingAddressDistrict = string.Empty;
            }
        }

        private string? _deliveryInstructions;
        public string? DeliveryInstructions
        {
            get => _deliveryInstructions;
            set
            {
                if (SetProperty(ref _deliveryInstructions, value))
                    OnPropertyChanged(nameof(HasDeliveryInstructions));
            }
        }

        public bool HasDeliveryInstructions => !string.IsNullOrWhiteSpace(DeliveryInstructions);

        public IRelayCommand BeginEditCommand { get; }
        public IRelayCommand CancelEditCommand { get; }
        public IRelayCommand UseCustomerAddressCommand { get; }
        public IRelayCommand UseCustomerDeliveryInstructionsCommand { get; }
        public IRelayCommand CopyTrackingNumberCommand { get; }

        public OrderLogisticsViewModel(OrderDetailsDto order, IToastService toastService)
        {
            _toastService = toastService;

            _orderStatus = order.Status;
            _customerAddress = order.Customer.Address;
            _customerDeliveryInstructions = order.Customer.DeliveryInstructions;
            _originalLogistics = new UpdateOrderLogisticsCommand(
                order.CourierId,
                order.TrackingNumber,
                new ShippingAddressDto(
                    order.ShippingAddressStreet,
                    order.ShippingAddressCity,
                    order.ShippingAddressDistrict,
                    order.ShippingAddressState,
                    order.ShippingAddressPostalCode,
                    order.ShippingAddressCountry),
                order.DeliveryInstructions);

            CanEdit = order.Status < OrderStatus.Shipped && IsReadonly;

            IsCashOnDelivery = order.IsCashOnDelivery;
            CourierId = order.CourierId;
            ShippingAddressStreet = order.ShippingAddressStreet;
            ShippingAddressCity = order.ShippingAddressCity;
            ShippingAddressState = order.ShippingAddressState;
            ShippingAddressDistrict = order.ShippingAddressDistrict;
            ShippingAddressPostalCode = order.ShippingAddressPostalCode;
            ShippingAddressCountry = order.ShippingAddressCountry;
            DeliveryInstructions = order.DeliveryInstructions;
            TrackingNumber = order.TrackingNumber;

            BeginEditCommand = new RelayCommand(BeginEdit);
            CancelEditCommand = new RelayCommand(CancelEdit);
            UseCustomerAddressCommand = new RelayCommand(
                UseCustomerAddress,
                () => _customerAddress != null);
            UseCustomerDeliveryInstructionsCommand = new RelayCommand(
                UseCustomerDeliveryInstructions,
                () => !string.IsNullOrWhiteSpace(_customerDeliveryInstructions));
            CopyTrackingNumberCommand = new RelayCommand(CopyTrackingNumber);
        }

        private void CopyTrackingNumber()
        {
            if (string.IsNullOrWhiteSpace(TrackingNumber)) return;
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(TrackingNumber);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            _toastService.ShowSuccess("Copied", "Tracking number copied to clipboard.");
        }

        private void BeginEdit()
        {
            //RestoreOriginalValues();
            IsEditing = true;
        }

        private void CancelEdit()
        {
            RestoreOriginalValues();
            IsEditing = false;
        }

        private void RestoreOriginalValues()
        {
            CourierId = _originalLogistics.CourierId;
            SelectedCourier = Couriers.FirstOrDefault(c => c.Id == CourierId);

            ShippingAddressStreet = _originalLogistics.ShippingAddress?.Street ?? string.Empty;
            ShippingAddressCity = _originalLogistics.ShippingAddress?.City ?? string.Empty;
            ShippingAddressState = _originalLogistics.ShippingAddress?.State ?? string.Empty;
            ShippingAddressDistrict = _originalLogistics.ShippingAddress?.District ?? string.Empty;
            ShippingAddressPostalCode = _originalLogistics.ShippingAddress?.PostalCode ?? string.Empty;
            ShippingAddressCountry = _originalLogistics.ShippingAddress?.Country ?? string.Empty;

            DeliveryInstructions = _originalLogistics.DeliveryInstructions;
        }

        public UpdateOrderLogisticsCommand? GetUpdateDto()
        {
            if(_orderStatus >= OrderStatus.Shipped)
            {
                _toastService.ShowError("Validation Error","Cannot update logistics information for orders that have already been shipped.");
                return null;
            }

            var shippingAdress = new ShippingAddressDto(
                    ShippingAddressStreet,
                    ShippingAddressCity,
                    ShippingAddressDistrict,
                    ShippingAddressState,
                    ShippingAddressPostalCode,
                    ShippingAddressCountry);

            return new UpdateOrderLogisticsCommand(CourierId, TrackingNumber, shippingAdress, DeliveryInstructions);
        }

        public void CommitEdit()
        {
            _originalLogistics = new UpdateOrderLogisticsCommand(
                CourierId,
                TrackingNumber,
                new ShippingAddressDto(
                    ShippingAddressStreet,
                    ShippingAddressCity,
                    ShippingAddressDistrict,
                    ShippingAddressState,
                    ShippingAddressPostalCode,
                    ShippingAddressCountry),
                DeliveryInstructions);
            IsEditing = false;
        }

        private void UseCustomerAddress()
        {
            if (_customerAddress == null) return;

            ShippingAddressStreet = _customerAddress.Street ?? string.Empty;
            ShippingAddressCity = _customerAddress.City ?? string.Empty;
            ShippingAddressState = _customerAddress.State ?? string.Empty;
            ShippingAddressDistrict = _customerAddress.District ?? string.Empty;
            ShippingAddressPostalCode = _customerAddress.PostalCode ?? string.Empty;
            ShippingAddressCountry = _customerAddress.Country ?? string.Empty;
        }

        private void UseCustomerDeliveryInstructions()
        {
            if (_customerDeliveryInstructions == null) return;
            DeliveryInstructions = _customerDeliveryInstructions ?? null;
        }
    }
}
