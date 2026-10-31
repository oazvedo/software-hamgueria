using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.FunctionalTests.Infrastructure;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(HamgueriaApiFactory))]

namespace SoftwareHamgueria.FunctionalTests.Infrastructure;

/// <summary>
/// The real API (migrations + seed on startup) on a throwaway Postgres, one container per test run.
/// RabbitMQ is replaced by an in-memory publisher. Tests isolate their data with unique comanda names
/// instead of wiping the database, so they can run in any order.
/// </summary>
public sealed class HamgueriaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public FakeOrderEventPublisher Publisher { get; } = new();

    public string ConnectionString => _postgres.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        // Build the host once, before tests run. Test classes run in parallel and WebApplicationFactory is not
        // thread-safe: concurrent CreateClient() calls each built a host, and each one ran migrations + seed.
        _ = Server;
    }

    /// <summary>Stops this factory's Postgres, to test how the API behaves without its database.</summary>
    public Task StopDatabaseAsync() => _postgres.StopAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Postgres", ConnectionString);
        builder.UseSetting("OTEL_EXPORTER_OTLP_ENDPOINT", ""); // no collector in tests (ADR 0028)
        // No broker in these tests: events go to the fake publisher, and the background sender (ADR 0025) points at a
        // closed port so it never reaches a developer's local RabbitMQ. BrokerApiFactory covers the real broker.
        builder.UseSetting("RabbitMq:Host", "127.0.0.1");
        builder.UseSetting("RabbitMq:Port", "1");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IOrderEventPublisher>();
            services.AddSingleton<IOrderEventPublisher>(Publisher);
            services.AddControllers().AddApplicationPart(typeof(BoomController).Assembly);
        });
    }

    public ApiClient CreateApiClient() =>
        new(CreateClient(), Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);
}
