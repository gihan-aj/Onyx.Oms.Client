using Refit;
using System;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Settings
{
    public interface ISettingsApi
    {
        [Get("/api/v1/settings/profile")]
        Task<TenantProfileResponse> GetTenantProfile();

        [Put("/api/v1/settings/profile/store-info")]
        Task UpdateStoreInfo([Body] UpdateStoreInfoCommand dto);

        [Put("/api/v1/settings/profile/hero-image")]
        Task UpdateHeroImage([Body] UpdateTenantHeroImageCommand dto);

        [Put("/api/v1/settings/profile/logo")]
        Task UpdateLogoImage([Body] UpdateTenantLogoCommand dto);

        [Put("/api/v1/settings/profile/regional-settings")]
        Task UpdateRegionalSettings([Body] UpdateRegionalSettingsCommand dto);

        [Put("/api/v1/settings/profile/address")]
        Task UpdateStoreAddress([Body] UpdateStoreAddressCommand dto);

        [Put("/api/v1/settings/profile/preferences")]
        Task UpdatePreferences([Body] UpdatePreferencesCommand dto);

        [Get("/api/v1/settings/sequences/{id}")]
        Task<int> GetSequenceValue(string id);

        [Put("/api/v1/settings/sequences/{id}")]
        Task UpdateSequenceValue(string id, [Body] int value);

        [Get("/api/v1/settings/whatsapp")]
        Task<WhatsAppSettingsDto> GetWhatsAppSettings();

        [Put("/api/v1/settings/whatsapp")]
        Task UpdateWhatsAppSettings([Body] UpdateWhatsAppSettingsCommand dto);

        [Post("/api/v1/settings/whatsapp/test")]
        Task TestWhatsAppConnection([Body] TestWhatsAppConnectionCommand dto);
    }

    public record TenantProfileResponse
    {
        public Guid Id { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public string? LegalName { get; set; }
        public string? TaxRegistrationNumber { get; set; }
        public AddressDto? StoreAddress { get; set; }
        public string DefaultCurrency {  get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public string WeightUnit { get; set; } = string.Empty;
        public string? InvoiceFooterText { get; set; }
        public string? LogoUrl { get; set; }
        public string? HeroImageUrl { get; set; }
        public string PreferencesJson { get; set; } = "{}";
    }

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
            if (!string.IsNullOrWhiteSpace(Street) && !string.IsNullOrWhiteSpace(City) && !string.IsNullOrWhiteSpace(District))
            {
                return $"{Street}, {City}, {District}";
            }
            else if (!string.IsNullOrWhiteSpace(City) && !string.IsNullOrWhiteSpace(District))
            {
                return $"{City}, {District}";
            }
            else if (!string.IsNullOrWhiteSpace(Street) && !string.IsNullOrWhiteSpace(City))
            {
                return $"{Street}, {City}";
            }
            else if (!string.IsNullOrWhiteSpace(Street) && !string.IsNullOrWhiteSpace(District))
            {
                return $"{Street}, {District}";
            }
            else if (!string.IsNullOrWhiteSpace(Street))
            {
                return Street;
            }
            else if (!string.IsNullOrWhiteSpace(City))
            {
                return City;
            }
            else if (!string.IsNullOrWhiteSpace(District))
            {
                return District;
            }
            else if (!string.IsNullOrWhiteSpace(State))
            {
                return State;
            }

            return "-";
        }
    }

    public record UpdateStoreInfoCommand(
        string StoreName,
        string? LegalName,
        string? TaxRegistrationNumber,
        string ContactEmail,
        string? ContactPhone,
        string? InvoiceFooterText
    );

    public record UpdateRegionalSettingsCommand(
        string DefaultCurrency,
        string TimeZone,
        string WeightUnit
    );

    public record UpdateStoreAddressCommand(
        AddressDto StoreAddress
    );

    public record UpdatePreferencesCommand(
        string PreferencesJson
    );

    public record UpdateTenantHeroImageCommand(string HeroImageUrl);

    public record UpdateTenantLogoCommand(string LogoUrl);

    public record WhatsAppSettingsDto(string? PhoneNumberId, bool IsConfigured);

    public record UpdateWhatsAppSettingsCommand(string PhoneNumberId, string? AccessToken);

    public record TestWhatsAppConnectionCommand(string DestinationPhone);
}
