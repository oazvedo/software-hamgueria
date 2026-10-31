using System.Net;
using System.Net.Http.Json;
using Npgsql;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class IdleComandasTests(HamgueriaApiFactory factory)
{
    private async Task OrderAtAsync(Guid tableId, string customer, TimeSpan age)
    {
        await using var db = new NpgsqlConnection(factory.ConnectionString);
        await db.OpenAsync();
        await using var insert = new NpgsqlCommand("""
            INSERT INTO "Orders" ("Id", "TableId", "CustomerName", "Status", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), @t, @c, @s, now() - @age, now() - @age)
            """, db);
        insert.Parameters.AddWithValue("t", tableId);
        insert.Parameters.AddWithValue("c", customer);
        insert.Parameters.AddWithValue("s", (int)OrderStatus.Done);
        insert.Parameters.AddWithValue("age", age);
        await insert.ExecuteNonQueryAsync();
    }

    [Fact(DisplayName = "025/CA-01, CA-02: comanda aberta parada há horas aparece para a cozinha; recente ou fechada, não")]
    public async Task IdleComandas_AreListed()
    {
        var kitchen = factory.CreateApiClient();
        await kitchen.LoginAsAdminAsync();
        var table = await kitchen.FirstTableIdAsync();
        var forgotten = ApiClient.UniqueCustomer("Esquecida");
        var recent = ApiClient.UniqueCustomer("Recente");
        var closed = ApiClient.UniqueCustomer("Fechada");
        await OrderAtAsync(table, forgotten, TimeSpan.FromDays(3)); // older than the kitchen list's 24 h window
        await OrderAtAsync(table, recent, TimeSpan.FromHours(1));
        await OrderAtAsync(table, closed, TimeSpan.FromHours(10));
        (await kitchen.CloseComandaAsync(table, closed)).EnsureSuccessStatusCode();

        var idle = (await kitchen.Http.GetFromJsonAsync<List<IdleComandaDto>>("/api/order/comandas/idle?hours=6", kitchen.Json))!;

        var item = Assert.Single(idle, c => c.CustomerName == forgotten);
        Assert.Equal(1, item.Orders);
        Assert.False(string.IsNullOrEmpty(item.TableName));
        Assert.DoesNotContain(idle, c => c.CustomerName == recent);
        Assert.DoesNotContain(idle, c => c.CustomerName == closed);
    }

    [Fact(DisplayName = "025/CA-03 (falha): sem login da cozinha, a lista é negada; horas fora de 1 a 72, recusadas")]
    public async Task IdleComandas_Guards()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateApiClient().Http.GetAsync("/api/order/comandas/idle")).StatusCode);
        var kitchen = factory.CreateApiClient();
        await kitchen.LoginAsAdminAsync();

        var response = await kitchen.Http.GetAsync("/api/order/comandas/idle?hours=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["As horas vão de 1 a 72."], (await kitchen.ReadProblemAsync(response)).Errors!["hours"]);
    }
}
