using System.Net.Http.Json;
using Npgsql;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class KitchenListTests(HamgueriaApiFactory factory)
{
    private async Task<Guid> InsertOrderAsync(string customer, int status, TimeSpan age)
    {
        var id = Guid.NewGuid();
        var tableId = await factory.CreateApiClient().FirstTableIdAsync();
        await using var db = new NpgsqlConnection(factory.ConnectionString);
        await db.OpenAsync();
        await using var insert = new NpgsqlCommand("""
            INSERT INTO "Orders" ("Id", "TableId", "CustomerName", "Status", "CreatedAt", "UpdatedAt")
            VALUES (@id, @table, @customer, @status, now() - @age, now() - @age)
            """, db);
        insert.Parameters.AddWithValue("id", id);
        insert.Parameters.AddWithValue("table", tableId);
        insert.Parameters.AddWithValue("customer", customer);
        insert.Parameters.AddWithValue("status", status);
        insert.Parameters.AddWithValue("age", age);
        await insert.ExecuteNonQueryAsync();
        return id;
    }

    [Fact(DisplayName = "020/CA-01, CA-02: a cozinha vê os pedidos em andamento de qualquer idade e os finalizados das últimas 24 h")]
    public async Task KitchenList_IsBounded()
    {
        // Stored as the enum's int: the test reads it from the enum instead of hard-coding the order.
        var oldActive = await InsertOrderAsync(ApiClient.UniqueCustomer("Velho"), (int)OrderStatus.Created, TimeSpan.FromDays(3));
        var oldDone = await InsertOrderAsync(ApiClient.UniqueCustomer("Velho"), (int)OrderStatus.Done, TimeSpan.FromDays(3));
        var recentDone = await InsertOrderAsync(ApiClient.UniqueCustomer("Recente"), (int)OrderStatus.Done, TimeSpan.FromHours(2));
        var kitchen = factory.CreateApiClient();
        await kitchen.LoginAsAdminAsync();

        var ids = (await kitchen.Http.GetFromJsonAsync<List<OrderResponseDto>>("/api/order", kitchen.Json))!.Select(o => o.Id).ToList();

        Assert.Contains(oldActive, ids);
        Assert.Contains(recentDone, ids);
        Assert.DoesNotContain(oldDone, ids);
        Assert.True(ids.Count <= SoftwareHamgueria.Application.Services.OrderService.KitchenMaxOrders);
    }
}
