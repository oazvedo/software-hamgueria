using System.Net;
using Microsoft.EntityFrameworkCore;
using SoftwareHamgueria.FunctionalTests.Infrastructure;
using SoftwareHamgueria.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace SoftwareHamgueria.FunctionalTests;

public class SeedAndComandaTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "019/CA-01: duas instâncias semeando ao mesmo tempo não duplicam o cardápio")]
    public async Task ConcurrentSeeds_DoNotDuplicate()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await postgres.StartAsync();
        AppDbContext NewContext() => new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(postgres.GetConnectionString()).Options);
        await using (var db = NewContext()) await db.Database.MigrateAsync();

        await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
        {
            using var db = NewContext();
            DatabaseSeeder.Seed(db);
        })));

        await using var check = NewContext();
        Assert.Equal(await check.Dishes.Select(d => d.Name).Distinct().CountAsync(), await check.Dishes.CountAsync());
        Assert.Equal(8, await check.Tables.CountAsync());
    }

    [Fact(DisplayName = "019/CA-02 (falha): fechar comanda de mesa que não existe é recusado com 404")]
    public async Task CloseComanda_UnknownTable_Returns404()
    {
        var api = factory.CreateApiClient();

        var response = await api.CloseComandaAsync(Guid.NewGuid(), "Ana");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("A mesa informada não existe.", (await api.ReadProblemAsync(response)).Detail);
    }
}
