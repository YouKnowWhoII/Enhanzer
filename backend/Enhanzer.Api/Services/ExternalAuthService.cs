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
    private const string SuccessfulLoginMessage = "GetLoginData POS API Executed Successfully.";

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

        if (loginResponse.StatusCode is not null and not 200)
        {
            throw new UnauthorizedAccessException(GetLoginFailureMessage(loginResponse));
        }

        if (loginResponse.UserLocations.Count == 0)
        {
            logger.LogWarning("External login response did not include User_Locations. StatusCode: {StatusCode}.", (int)response.StatusCode);
            throw new UnauthorizedAccessException(GetMissingLocationMessage(loginResponse));
        }

        return loginResponse;
    }

    private static ExternalLoginResponse? ParseLoginResponse(string content)
    {
        using var document = JsonDocument.Parse(content);
        var statusCode = FindInt(document.RootElement, "Status_Code", "statusCode");
        var message = FindString(document.RootElement, "Message", "message");
        var documentMessage = FindString(document.RootElement, "Doc_Msg", "docMsg");

        // User_Locations is nested inside Response_Body in the staging response, so parsing is intentionally tolerant.
        var locations = FindLocations(document.RootElement);

        return new ExternalLoginResponse
        {
            StatusCode = statusCode,
            Message = message,
            DocumentMessage = documentMessage,
            UserLocations = locations
        };
    }

    private static string GetLoginFailureMessage(ExternalLoginResponse response)
    {
        if (!string.IsNullOrWhiteSpace(response.DocumentMessage))
        {
            return response.DocumentMessage;
        }

        return string.IsNullOrWhiteSpace(response.Message) ? "Invalid email or password." : response.Message;
    }

    private static string GetMissingLocationMessage(ExternalLoginResponse response)
    {
        if (!string.IsNullOrWhiteSpace(response.DocumentMessage))
        {
            return response.DocumentMessage;
        }

        if (string.IsNullOrWhiteSpace(response.Message) || !string.Equals(response.Message, SuccessfulLoginMessage, StringComparison.OrdinalIgnoreCase))
        {
            return GetLoginFailureMessage(response);
        }

        return "Login succeeded, but no locations were returned for this user.";
    }

    private static string? FindString(JsonElement element, params string[] names)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nestedValue = FindString(item, names);
                if (!string.IsNullOrWhiteSpace(nestedValue))
                {
                    return nestedValue;
                }
            }

            return null;
        }

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

    private static int? FindInt(JsonElement element, params string[] names)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nestedValue = FindInt(item, names);
                if (nestedValue.HasValue)
                {
                    return nestedValue;
                }
            }

            return null;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (names.Any(name => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                && property.Value.ValueKind == JsonValueKind.Number
                && property.Value.TryGetInt32(out var value))
            {
                return value;
            }

            var nestedValue = FindInt(property.Value, names);
            if (nestedValue.HasValue)
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
