using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SoftwareHamgueria.Application.DTOs.Auth;
using SoftwareHamgueria.Application.Services.Interfaces;

namespace SoftwareHamgueria.Application.Services;

public class AuthService(IConfiguration configuration) : IAuthService
{
    /// <returns>The token, or <c>null</c> when the credentials are wrong.</returns>
    public TokenResponseDto? Login(LoginDto dto)
    {
        // No built-in fallback (ADR 0024): without configured credentials, nobody gets in.
        var adminUser = configuration["Auth:AdminUsername"];
        var adminPassword = configuration["Auth:AdminPassword"];

        if (string.IsNullOrEmpty(adminUser) || string.IsNullOrEmpty(adminPassword)
            || dto.Username != adminUser || dto.Password != adminPassword)
            return null;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Auth:JwtSecret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: configuration["Auth:Issuer"],
            audience: configuration["Auth:Audience"],
            claims: [new Claim(ClaimTypes.Role, "Admin"), new Claim(ClaimTypes.Name, dto.Username)],
            expires: expires,
            signingCredentials: creds);

        return new TokenResponseDto(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
