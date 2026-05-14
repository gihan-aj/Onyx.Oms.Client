using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Create;

public sealed partial class CreateNewCustomerDialog : ContentDialog, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    private string _state = string.Empty;
    public string State 
    { 
        get => _state; 
        set 
        {
            if (_state != value)
            {
                _state = value;
                OnPropertyChanged();
                UpdateDistricts(value);
            }
        }
    }
    
    private string _district = string.Empty;
    public string District
    {
        get => _district;
        set
        {
            if (_district != value)
            {
                _district = value;
                OnPropertyChanged();
            }
        }
    }

    private string[] _districts = Array.Empty<string>();
    public string[] Districts 
    { 
        get => _districts; 
        private set 
        {
            _districts = value;
            OnPropertyChanged();
        }
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

        if (!string.IsNullOrWhiteSpace(District) && Array.IndexOf(Districts, District) == -1)
        {
            District = string.Empty;
        }
    }

    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Sri Lanka";
    public string? DeliveryInstructions { get; set; }
    public string? Notes { get; set; }

    public CreateNewCustomerDialog(CreateCustomerCommand? customerDraft)
    {
        InitializeComponent();

        if(customerDraft != null)
        {
            CustomerName = customerDraft.Name;
            Email = customerDraft.Email ?? string.Empty;
            PrimaryPhone = customerDraft.PrimaryPhone;
            SecondaryPhone = customerDraft.SecondaryPhone ?? string.Empty;
            Street = customerDraft.Street ?? string.Empty;
            City = customerDraft.City ?? string.Empty;
            State = customerDraft.State ?? string.Empty;
            District = customerDraft.District ?? string.Empty;
            PostalCode = customerDraft.PostalCode ?? string.Empty;
            Country = customerDraft.Country ?? string.Empty;
            Notes = customerDraft.Notes ?? string.Empty;
        }
    }
}
