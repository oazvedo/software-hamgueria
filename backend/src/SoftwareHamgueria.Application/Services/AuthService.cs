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
        var adminUser = configuration["Auth:AdminUsername"] ?? "admin";
        var adminPassword = configuration["Auth:AdminPassword"] ?? "admin123";

        if (dto.Username != adminUser || dto.Password != adminPassword)
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
