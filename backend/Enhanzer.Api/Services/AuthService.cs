using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Enhanzer.Api.DTOs;
using Enhanzer.Api.Helpers;
using Enhanzer.Api.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Enhanzer.Api.Services;

public sealed class AuthService(
    IExternalAuthService externalAuthService,
    ILocationRepository locationRepository,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var externalResponse = await externalAuthService.LoginAsync(request, cancellationToken);
        var locations = await locationRepository.UpsertAsync(externalResponse.UserLocations, cancellationToken);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes);

        return new LoginResponse
        {
            Token = CreateToken(request.Email, expiresAtUtc),
            ExpiresAtUtc = expiresAtUtc,
            Locations = locations
        };
    }

    private string CreateToken(string email, DateTime expiresAtUtc)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
