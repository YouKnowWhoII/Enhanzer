using System.Text.Json.Serialization;

namespace Enhanzer.Api.Models;

public sealed class ExternalLoginRequest
{
    [JsonPropertyName("API_Action")]
    public string ApiAction { get; init; } = "GetLoginData";

    [JsonPropertyName("Device_Id")]
    public string DeviceId { get; init; } = "D001";

    [JsonPropertyName("Sync_Time")]
    public string SyncTime { get; init; } = string.Empty;

    [JsonPropertyName("Company_Code")]
    public string CompanyCode { get; init; } = string.Empty;

    [JsonPropertyName("API_Body")]
    public ExternalLoginBody ApiBody { get; init; } = new();
}

public sealed class ExternalLoginBody
{
    [JsonPropertyName("Username")]
    public string Username { get; init; } = string.Empty;

    [JsonPropertyName("Pw")]
    public string Password { get; init; } = string.Empty;
}
