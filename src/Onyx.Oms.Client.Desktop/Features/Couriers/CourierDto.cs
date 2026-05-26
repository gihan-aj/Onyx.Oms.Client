using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public class CourierDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("contactPerson")]
    public string? ContactPerson { get; set; }

    [JsonPropertyName("primaryPhone")]
    public string? PrimaryPhone { get; set; }

    [JsonPropertyName("secondaryPhone")]
    public string? SecondaryPhone { get; set; }

    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }

    [JsonPropertyName("zoneRates")]
    public List<CourierZoneRateDto> ZoneRates { get; set; } = new List<CourierZoneRateDto>();

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    // Permission flags (Not mapped from API, populated locally)
    [JsonIgnore]
    public bool CanView { get; set; }

    [JsonIgnore]
    public bool CanEdit { get; set; }

    [JsonIgnore]
    public bool CanDelete { get; set; }

    [JsonIgnore]
    public bool CanToggleStatus { get; set; }
}

public class CourierZoneRateDto
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }

    [JsonPropertyName("zoneName")]
    public string ZoneName { get; set; } = string.Empty;

    [JsonPropertyName("baseFee")]
    public decimal BaseFee { get; set; }

    [JsonPropertyName("baseFeeCurrency")]
    public string BaseFeeCurrency { get; set; } = string.Empty;

    [JsonPropertyName("baseWeight")]
    public decimal BaseWeight { get; set; }

    [JsonPropertyName("baseWeightUnit")]
    public string BaseWeightUnit { get; set; } = string.Empty;

    [JsonPropertyName("excessFeePerWeightUnit")]
    public decimal ExcessFeePerWeightUnit { get; set; }

    [JsonPropertyName("excessFeePerWeightUnitCurrency")]
    public string ExcessFeePerWeightUnitCurrency { get; set; } = string.Empty;

    [JsonPropertyName("codPercentage")]
    public decimal CodPercentage { get; set; }

    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }

    [JsonPropertyName("coveredDistricts")]
    public IReadOnlyCollection<string> CoveredDistricts { get; set; } = Array.Empty<string>();
}
