using System.ComponentModel.DataAnnotations;

namespace Enhanzer.Api.DTOs;

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string Password { get; init; } = string.Empty;
}
