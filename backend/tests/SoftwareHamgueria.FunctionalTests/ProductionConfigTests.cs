using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SoftwareHamgueria.FunctionalTests;

public class ProductionConfigTests
{
    private sealed class ProductionWithoutAdminPassword : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:Postgres", "Host=localhost;Port=1;Database=x;Username=x;Password=x");
            builder.UseSetting("Auth:JwtSecret", new string('k', 32));
            builder.UseSetting("RabbitMq:Username", "hamgueria");
            builder.UseSetting("RabbitMq:Password", "fila-secreta");
            // Auth:AdminPassword deliberately missing; Auth:AdminUsername comes from appsettings.json.
        }
    }

    [Fact(DisplayName = "004/CA-04: em produção sem a senha da cozinha, a API não sobe e cita a chave")]
    public void Production_WithoutAdminPassword_DoesNotStart()
    {
        using var factory = new ProductionWithoutAdminPassword();

        var error = Assert.ThrowsAny<Exception>(() => factory.Server);

        var messages = string.Join(" | ", Chain(error).Select(e => e.Message));
        Assert.Contains("Auth:AdminPassword", messages);
        Assert.DoesNotContain("fila-secreta", messages);
    }

    private static IEnumerable<Exception> Chain(Exception? e)
    {
        for (; e is not null; e = e.InnerException) yield return e;
    }
}
