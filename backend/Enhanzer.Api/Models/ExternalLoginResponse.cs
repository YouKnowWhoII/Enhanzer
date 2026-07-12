using System.Text.Json.Serialization;

namespace Enhanzer.Api.Models;

public sealed class ExternalLoginResponse
{
    [JsonPropertyName("Status_Code")]
    public int? StatusCode { get; init; }

    [JsonPropertyName("Status")]
    public string? Status { get; init; }

    [JsonPropertyName("Message")]
    public string? Message { get; init; }

    [JsonPropertyName("Doc_Msg")]
    public string? DocumentMessage { get; init; }

    [JsonPropertyName("User_Locations")]
    public List<ExternalLocation> UserLocations { get; init; } = [];
}

public sealed class ExternalLocation
{
    [JsonPropertyName("Location_Code")]
    public string LocationCode { get; init; } = string.Empty;

    [JsonPropertyName("Location_Name")]
    public string LocationName { get; init; } = string.Empty;
}
