using CommunityToolkit.Mvvm.ComponentModel;

namespace Onyx.Oms.Client.Desktop.Shared.Models;

public partial class TenantProfileDto : ObservableObject
{
    private string _id = string.Empty;
    public string Id { get => _id; set => SetProperty(ref _id, value); }
    
    private string _storeName = string.Empty;
    public string StoreName { get => _storeName; set => SetProperty(ref _storeName, value); }
    
    private string? _legalName;
    public string? LegalName { get => _legalName; set => SetProperty(ref _legalName, value); }
    
    private string? _taxRegistrationNumber;
    public string? TaxRegistrationNumber { get => _taxRegistrationNumber; set => SetProperty(ref _taxRegistrationNumber, value); }
    
    private string _contactEmail = string.Empty;
    public string ContactEmail { get => _contactEmail; set => SetProperty(ref _contactEmail, value); }
    
    private string? _contactPhone;
    public string? ContactPhone { get => _contactPhone; set => SetProperty(ref _contactPhone, value); }
    
    private AddressDto? _storeAddress;
    public AddressDto? StoreAddress { get => _storeAddress; set => SetProperty(ref _storeAddress, value); }
    
    private string _baseCurrency = "LKR";
    public string BaseCurrency { get => _baseCurrency; set => SetProperty(ref _baseCurrency, value); }
    
    private string _weightUnit = "kg";
    public string WeightUnit { get => _weightUnit; set => SetProperty(ref _weightUnit, value); }
    
    private string? _invoiceFooterText;
    public string? InvoiceFooterText { get => _invoiceFooterText; set => SetProperty(ref _invoiceFooterText, value); }
    
    private string? _logoUrl;
    public string? LogoUrl { get => _logoUrl; set => SetProperty(ref _logoUrl, value); }
    
    private string? _preferencesJson;
    public string? PreferencesJson { get => _preferencesJson; set => SetProperty(ref _preferencesJson, value); }
}

public partial class AddressDto : ObservableObject
{
    private string? _street;
    public string? Street { get => _street; set => SetProperty(ref _street, value); }
    
    private string? _city;
    public string? City { get => _city; set => SetProperty(ref _city, value); }
    
    private string? _state;
    public string? State { get => _state; set => SetProperty(ref _state, value); }
    
    private string? _postalCode;
    public string? PostalCode { get => _postalCode; set => SetProperty(ref _postalCode, value); }
    
    private string? _country;
    public string? Country { get => _country; set => SetProperty(ref _country, value); }
}

public class UpdateStoreInfoDto
{
    public string StoreName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
}

public class UpdateRegionalSettingsDto
{
    public string BaseCurrency { get; set; } = string.Empty;
    public string WeightUnit { get; set; } = string.Empty;
}

public class UpdateStoreAddressDto
{
    public AddressDto? StoreAddress { get; set; }
}

public class UpdatePreferencesDto
{
    public string? PreferencesJson { get; set; }
}
