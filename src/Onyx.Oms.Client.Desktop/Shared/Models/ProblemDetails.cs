using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Onyx.Oms.Client.Desktop.Shared.Models;

public class ProblemDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("extensions")]
    public ErrorExtensions? Extensions { get; set; }

    [JsonPropertyName("errors")]
    public List<ErrorDetail>? Errors { get; set; }
}

public class ErrorExtensions
{
    [JsonPropertyName("errors")]
    public List<ErrorDetail>? Errors { get; set; }
}

public class ErrorDetail
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
