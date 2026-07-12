using Enhanzer.Api.DTOs;
using Enhanzer.Api.Models;

namespace Enhanzer.Api.Services;

public interface IExternalAuthService
{
    Task<ExternalLoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
