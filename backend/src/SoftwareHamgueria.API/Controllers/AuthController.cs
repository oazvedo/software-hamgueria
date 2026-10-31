using Microsoft.AspNetCore.Mvc;
using SoftwareHamgueria.API.Errors;
using SoftwareHamgueria.Application.DTOs.Auth;
using SoftwareHamgueria.Application.Services.Interfaces;

namespace SoftwareHamgueria.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<TokenResponseDto> Login([FromBody] LoginDto dto)
    {
        var token = authService.Login(dto);
        return token is null
            ? Problem(statusCode: StatusCodes.Status401Unauthorized, type: ProblemTypes.Unauthorized,
                title: ProblemTypes.InvalidCredentialsTitle)
            : Ok(token);
    }
}
