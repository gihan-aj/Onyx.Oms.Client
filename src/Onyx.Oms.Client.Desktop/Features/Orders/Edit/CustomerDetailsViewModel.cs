using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class CustomerDetailsViewModel : ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _primaryPhone;
        public string PrimaryPhone
        {
            get => _primaryPhone;
            set => SetProperty(ref _primaryPhone, value);
        }

        private string? _email;
        public string? Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    OnPropertyChanged(nameof(HasEmail));
                }
            }
        }

        public bool HasEmail => !string.IsNullOrWhiteSpace(Email);

        private string? _secondaryPhone;
        public string? SecondaryPhone
        {
            get => _secondaryPhone;
            set
            {
                if (SetProperty(ref _secondaryPhone, value))
                    OnPropertyChanged(nameof(HasSecondaryPhone));
            }
        }
        public bool HasSecondaryPhone => !string.IsNullOrWhiteSpace(SecondaryPhone);

        private AddressDto? _address;
        public AddressDto? Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value))
                    OnPropertyChanged(nameof(HasAddress));
            }
        }
        public bool HasAddress => Address != null;

        private string? _deliveryInstructions;
        public string? DeliveryInstructions
        {
            get => _deliveryInstructions;
            set => SetProperty(ref _deliveryInstructions, value);
        }

        private string? _notes;
        public string? Notes
        {
            get => _notes;
            set
            {
                if (SetProperty(ref _notes, value))
                    OnPropertyChanged(nameof(HasNotes));
            }
        }
        public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        private DateTimeOffset _createdDate;
        public DateTimeOffset CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        public CustomerDetailsViewModel(CustomerDto dto)
        {
            _name = dto.Name;
            _primaryPhone = dto.PrimaryPhone;
            _email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
            _secondaryPhone = string.IsNullOrWhiteSpace(dto.SecondaryPhone) ? null : dto.SecondaryPhone;
            _address = dto.Address;
            _deliveryInstructions = dto.DeliveryInstructions;
            _notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;
            _isActive = dto.IsActive;
            _createdDate = dto.CreatedDate;
        }
    }
}
