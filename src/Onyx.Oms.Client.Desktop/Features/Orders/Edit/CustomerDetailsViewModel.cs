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

        public CustomerDetailsViewModel(CustomerDetailsDto dto)
        {
            _name = dto.Name;
            _primaryPhone = dto.PrimaryPhone;
            _email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
        }
    }
}
