using System.Net;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class HealthEndpointTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "004/CA-09: com o banco no ar, a verificação de saúde responde saudável")]
    public async Task Health_DatabaseUp_ReturnsHealthy()
    {
        var response = await factory.CreateApiClient().Http.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact(DisplayName = "004/CA-09: com o banco parado, a verificação responde indisponível sem detalhes")]
    public async Task Health_DatabaseDown_ReturnsUnhealthy()
    {
        // Own API + Postgres: stopping the shared database would break the other tests.
        await using var isolated = new HamgueriaApiFactory();
        await isolated.InitializeAsync();
        await isolated.StopDatabaseAsync();

        var response = await isolated.CreateApiClient().Http.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
    }
}
