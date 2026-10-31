using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class OutboxTests(HamgueriaApiFactory factory)
{
    private async Task ExecAsync(string sql)
    {
        await using var db = new NpgsqlConnection(factory.ConnectionString);
        await db.OpenAsync();
        await using var command = new NpgsqlCommand(sql, db);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<long> ScalarAsync(string sql, params (string Name, object Value)[] args)
    {
        await using var db = new NpgsqlConnection(factory.ConnectionString);
        await db.OpenAsync();
        await using var command = new NpgsqlCommand(sql, db);
        foreach (var (name, value) in args) command.Parameters.AddWithValue(name, value);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    [Fact(DisplayName = "017/CA-01: criar o pedido grava o evento no outbox, na mesma transação")]
    public async Task CreatingAnOrder_StagesItsEvent()
    {
        var order = await factory.CreateApiClient().CreateOrderAsync(ApiClient.UniqueCustomer("Outbox"));

        Assert.Equal(1, factory.Publisher.Count("order.created", order.Id.ToString()));
    }

    [Fact(DisplayName = "017/CA-02 (falha): se gravar o pedido falha, o evento também não fica")]
    public async Task FailedWrite_LeavesNoEvent()
    {
        var customer = ApiClient.UniqueCustomer("Atomico");
        // Test-only trigger that makes this one customer's insert fail inside the transaction.
        var fn = $"test_fail_{Guid.NewGuid():N}";
        await ExecAsync($"""
            CREATE FUNCTION {fn}() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN IF NEW."CustomerName" = '{customer}' THEN RAISE EXCEPTION 'falha de teste'; END IF; RETURN NEW; END $$;
            CREATE TRIGGER {fn} BEFORE INSERT ON "Orders" FOR EACH ROW EXECUTE FUNCTION {fn}();
            """);
        try
        {
            var api = factory.CreateApiClient();
            var response = await api.PostOrderAsync(await api.BuildOrderAsync(customer));

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            // No event may point at an order that does not exist.
            Assert.Equal(0, await ScalarAsync("""
                SELECT count(*) FROM "OutboxMessages" m
                LEFT JOIN "Orders" o ON o."Id"::text = m."Payload"
                WHERE o."Id" IS NULL
                """));
        }
        finally
        {
            await ExecAsync($"""DROP TRIGGER IF EXISTS {fn} ON "Orders"; DROP FUNCTION IF EXISTS {fn}();""");
        }
    }
}

public class OutboxRetentionTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "017/CA-04: a limpeza apaga evento enviado há mais de 7 dias e nunca um não enviado")]
    public async Task Retention_DeletesOldSentOnly()
    {
        var marker = Guid.NewGuid().ToString();
        await using (var db = new NpgsqlConnection(factory.ConnectionString))
        {
            await db.OpenAsync();
            await using var insert = new NpgsqlCommand("""
                INSERT INTO "OutboxMessages" ("Id", "RoutingKey", "Payload", "CreatedAt", "SentAt", "Attempts") VALUES
                    (gen_random_uuid(), 'old-sent', @m, now() - interval '30 days', now() - interval '30 days', 0),
                    (gen_random_uuid(), 'old-unsent', @m, now() - interval '30 days', NULL, 0),
                    (gen_random_uuid(), 'recent-sent', @m, now(), now() - interval '1 day', 0)
                """, db);
            insert.Parameters.AddWithValue("m", marker);
            await insert.ExecuteNonQueryAsync();
        }

        await using (var scope = factory.Services.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<SoftwareHamgueria.Infrastructure.Jobs.OutboxRetentionJob>().RunAsync();

        Assert.False(factory.Publisher.WasPublished("old-sent", marker));
        Assert.True(factory.Publisher.WasPublished("old-unsent", marker));
        Assert.True(factory.Publisher.WasPublished("recent-sent", marker));
    }
}

public class OutboxDeliveryTests
{
    [Fact(DisplayName = "017/CA-03: evento que já estava no outbox (de antes de um reinício) é enviado e marcado")]
    public async Task StoredEvent_IsSentAndMarked()
    {
        await using var api = await BrokerApiFactory.StartAsync(brokerUp: true);
        var marker = Guid.NewGuid().ToString();
        await using (var db = new NpgsqlConnection(api.PostgresConnectionString))
        {
            await db.OpenAsync();
            await using var insert = new NpgsqlCommand("""
                INSERT INTO "OutboxMessages" ("Id", "RoutingKey", "Payload", "CreatedAt", "Attempts")
                VALUES (gen_random_uuid(), 'order.created', @p, now(), 0)
                """, db);
            insert.Parameters.AddWithValue("p", marker);
            await insert.ExecuteNonQueryAsync();
        }

        Assert.Contains(("order.created", marker), await api.ReceiveEventsAsync(1));
        await BrokerApiFactory.Eventually(async () =>
        {
            await using var db = new NpgsqlConnection(api.PostgresConnectionString);
            await db.OpenAsync();
            await using var query = new NpgsqlCommand("""SELECT "SentAt" IS NOT NULL FROM "OutboxMessages" WHERE "Payload" = @p""", db);
            query.Parameters.AddWithValue("p", marker);
            return (bool)(await query.ExecuteScalarAsync())!;
        }, TimeSpan.FromSeconds(15), "a linha do outbox devia ficar marcada como enviada");
    }
}
