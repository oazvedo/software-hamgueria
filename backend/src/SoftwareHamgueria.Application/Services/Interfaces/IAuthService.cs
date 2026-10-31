using SoftwareHamgueria.Application.DTOs.Auth;

namespace SoftwareHamgueria.Application.Services.Interfaces;

public interface IAuthService
{
    TokenResponseDto? Login(LoginDto dto);
}
