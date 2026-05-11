using System;

namespace Onyx.Oms.Client.Desktop.Features.Customers;

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    public override string ToString()
    {
        if(!string.IsNullOrWhiteSpace(Street) && !string.IsNullOrWhiteSpace(City) && !string.IsNullOrWhiteSpace(District))
        {
            return $"{Street}, {City}, {District}";
        }
        else if(!string.IsNullOrWhiteSpace(City) && !string.IsNullOrWhiteSpace(District))
        {
            return $"{City}, {District}";
        }
        else if(!string.IsNullOrWhiteSpace(Street) && !string.IsNullOrWhiteSpace(City))
        {
            return $"{Street}, {City}";
        }
        else if(!string.IsNullOrWhiteSpace(Street) && !string.IsNullOrWhiteSpace(District))
        {
            return $"{Street}, {District}";
        }
        else if(!string.IsNullOrWhiteSpace(Street))
        {
            return Street;
        }
        else if(!string.IsNullOrWhiteSpace(City))
        {
            return City;
        }
        else if(!string.IsNullOrWhiteSpace(District))
        {
            return District;
        }
        else if(!string.IsNullOrWhiteSpace(State))
        {
            return State;
        }

        return "-";
    }
}

public class CustomerDto
{
    private string _lastOrderNumber = "-";

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public AddressDto? Address { get; set; }
    public string? LastOrderNumber { 
        get => string.IsNullOrWhiteSpace(_lastOrderNumber) ? "-" : _lastOrderNumber;
        set => _lastOrderNumber = string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    // UI Permission Flags
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanActivate { get; set; }
    public bool CanDeactivate { get; set; }
}

public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateCustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
