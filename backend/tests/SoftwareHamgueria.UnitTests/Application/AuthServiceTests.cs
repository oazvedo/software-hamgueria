using Microsoft.Extensions.Configuration;
using NSubstitute;
using SoftwareHamgueria.Application.DTOs.Auth;
using SoftwareHamgueria.Application.Services;

namespace SoftwareHamgueria.UnitTests.Application;

public class AuthServiceTests
{
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var config = Substitute.For<IConfiguration>();
        config["Auth:AdminUsername"].Returns("admin");
        config["Auth:AdminPassword"].Returns("admin123");
        config["Auth:JwtSecret"].Returns(new string('s', 64));
        config["Auth:Issuer"].Returns("test");
        config["Auth:Audience"].Returns("test");
        _sut = new AuthService(config);
    }

    [Fact(DisplayName = "001/CA-10: credenciais erradas da cozinha não geram token")]
    public void Login_WrongPassword_ReturnsNull()
    {
        Assert.Null(_sut.Login(new LoginDto("admin", "errada")));
    }

    [Fact(DisplayName = "001/CA-10: credenciais certas da cozinha geram token")]
    public void Login_RightCredentials_ReturnsToken()
    {
        Assert.False(string.IsNullOrEmpty(_sut.Login(new LoginDto("admin", "admin123"))?.Token));
    }
}
