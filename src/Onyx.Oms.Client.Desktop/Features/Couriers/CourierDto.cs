using System;
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
