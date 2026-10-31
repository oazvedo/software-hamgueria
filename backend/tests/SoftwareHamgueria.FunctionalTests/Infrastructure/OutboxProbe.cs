using Npgsql;

namespace SoftwareHamgueria.FunctionalTests.Infrastructure;

/// <summary>
/// Reads the real outbox table (ADR 0031): an event "was published" when its row exists, written in the same
/// transaction as the order. Replaces the in-memory fake publisher the tests used before the outbox.
/// </summary>
public sealed class OutboxProbe(Func<string> connectionString)
{
    public bool WasPublished(string routingKey, string message) => Count(routingKey, message) > 0;

    public long Count(string routingKey, string message)
    {
        using var db = new NpgsqlConnection(connectionString());
        db.Open();
        using var query = new NpgsqlCommand(
            """SELECT count(*) FROM "OutboxMessages" WHERE "RoutingKey" = @k AND "Payload" = @m""", db);
        query.Parameters.AddWithValue("k", routingKey);
        query.Parameters.AddWithValue("m", message);
        return (long)query.ExecuteScalar()!;
    }
}
