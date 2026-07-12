using Enhanzer.Api.DTOs;

namespace Enhanzer.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
