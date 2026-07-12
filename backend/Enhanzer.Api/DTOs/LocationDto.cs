namespace Enhanzer.Api.DTOs;

public sealed class LocationDto
{
    public int Id { get; init; }
    public string LocationCode { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
}
