namespace Enhanzer.Api.DTOs;

public sealed class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public IReadOnlyCollection<LocationDto> Locations { get; init; } = [];
}
