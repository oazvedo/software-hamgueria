using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoftwareHamgueria.FunctionalTests.Infrastructure;
using SoftwareHamgueria.Infrastructure.Data;

namespace SoftwareHamgueria.FunctionalTests;

public class IsolationTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "002/CA-05: a API dos testes usa o Postgres descartável, nunca o banco de desenvolvimento")]
    public void Api_UsesThrowawayDatabase_NotTheDevelopmentOne()
    {
        using var scope = factory.Services.CreateScope();
        var used = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.GetConnectionString();

        Assert.Equal(factory.ConnectionString, used);
        Assert.DoesNotContain("Database=hamgueria;", used);
    }

    [Fact(DisplayName = "002/CA-01: a API sobe no banco descartável com o cardápio e as mesas semeados uma única vez")]
    public async Task Startup_AppliesMigrationsAndSeedOnce()
    {
        var api = factory.CreateApiClient();

        // Other tests add their own rows to the shared database, so assert the seed is present and not duplicated
        // instead of counting everything.
        var dishes = (await api.GetDishesAsync()).Select(d => d.Name).ToList();
        var tables = (await api.GetTablesAsync()).Select(t => t.TableName).ToList();

        Assert.Contains("Burger Monolito", dishes);
        Assert.Contains("Pudim CI/CD", dishes);
        Assert.Equal(dishes.Distinct().Count(), dishes.Count);
        Assert.Equal(Enumerable.Range(1, 8).Select(i => $"Mesa {i:D2}"), tables.Order());
    }
}
