using Microsoft.AspNetCore.Mvc;
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
        try
        {
            var token = authService.Login(dto);
            return Ok(token);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid credentials.");
        }
    }
}
