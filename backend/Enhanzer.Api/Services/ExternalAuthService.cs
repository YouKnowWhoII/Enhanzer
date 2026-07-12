using System.Net;
using System.Text;
using System.Text.Json;
using Enhanzer.Api.DTOs;
using Enhanzer.Api.Models;

namespace Enhanzer.Api.Services;

public sealed class ExternalAuthService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<ExternalAuthService> logger) : IExternalAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ExternalLoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var endpoint = configuration["ExternalAuth:Endpoint"] ?? throw new InvalidOperationException("External auth endpoint is not configured.");
        var externalRequest = new ExternalLoginRequest
        {
            CompanyCode = request.Email,
            ApiBody = new ExternalLoginBody
            {
                Username = request.Email,
                Password = request.Password
            }
        };

        var payload = JsonSerializer.Serialize(externalRequest, JsonOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        // The staging API is sensitive to browser-like headers, so the server mirrors the working manual request.
        httpRequest.Headers.Accept.ParseAdd("*/*");
        httpRequest.Headers.UserAgent.ParseAdd("Mozilla/5.0");

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Authentication service is temporarily unavailable.");
        }

        var loginResponse = ParseLoginResponse(content);
        if (loginResponse is null)
        {
            throw new InvalidOperationException("Authentication service returned an invalid response.");
        }

        if (loginResponse.UserLocations.Count == 0)
        {
            logger.LogWarning("External login response did not include User_Locations. StatusCode: {StatusCode}.", (int)response.StatusCode);
            throw new UnauthorizedAccessException("Login succeeded, but the authentication service did not return User_Locations.");
        }

        return loginResponse;
    }

    private static ExternalLoginResponse? ParseLoginResponse(string content)
    {
        using var document = JsonDocument.Parse(content);
        var message = FindString(document.RootElement, "Message", "message");

        // User_Locations is nested inside Response_Body in the staging response, so parsing is intentionally tolerant.
        var locations = FindLocations(document.RootElement);

        return new ExternalLoginResponse
        {
            Message = message,
            UserLocations = locations
        };
    }

    private static string? FindString(JsonElement element, params string[] names)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (names.Any(name => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                && property.Value.ValueKind == JsonValueKind.String)
            {
                return property.Value.GetString();
            }

            var nestedValue = FindString(property.Value, names);
            if (!string.IsNullOrWhiteSpace(nestedValue))
            {
                return nestedValue;
            }
        }

        return null;
    }

    private static List<ExternalLocation> FindLocations(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, "User_Locations", StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<ExternalLocation>>(property.Value.GetRawText(), JsonOptions) ?? [];
                }

                var nestedLocations = FindLocations(property.Value);
                if (nestedLocations.Count > 0)
                {
                    return nestedLocations;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nestedLocations = FindLocations(item);
                if (nestedLocations.Count > 0)
                {
                    return nestedLocations;
                }
            }
        }

        return [];
    }
}
