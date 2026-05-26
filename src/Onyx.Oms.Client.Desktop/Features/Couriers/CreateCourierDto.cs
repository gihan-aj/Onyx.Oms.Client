using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public class CreateCourierDto
{
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

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("zoneRates")]
    public List<CreateCourierZoneRateDto>? ZoneRates { get; set; }
}

public class CreateCourierZoneRateDto
{
    [JsonPropertyName("zoneName")]
    public string ZoneName { get; set; } = string.Empty;
    [JsonPropertyName("baseFee")]
    public decimal BaseFee { get; set; }
    [JsonPropertyName("baseWeight")]
    public decimal BaseWeight { get; set; }
    [JsonPropertyName("excessFeePerWeightUnit")]
    public decimal ExcessFeePerWeightUnit { get; set; }
    [JsonPropertyName("codPercentage")]
    public decimal CodPercentage { get; set; }
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    [JsonPropertyName("weightUnit")]
    public string WeightUnit { get; set; } = string.Empty;
    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }
    [JsonPropertyName("coveredDistricts")]
    public List<string> CoveredDistricts { get; set; } = new List<string>();
}
