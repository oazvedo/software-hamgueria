using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SoftwareHamgueria.Infrastructure.Messaging;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SoftwareHamgueria.FunctionalTests.Infrastructure;

/// <summary>
/// The real API with its real background sender (ADR 0025) against a RabbitMQ that the test stops and starts.
/// The broker keeps a fixed host port so the API finds it again after a restart. A durable test queue bound to
/// <c>order.#</c> collects what reaches the exchange. One instance per test: each scenario owns its broker.
/// </summary>
public sealed class BrokerApiFactory : WebApplicationFactory<Program>
{
    private const string TestQueue = "test.order-events";
    private const string User = "hamgueria";
    private const string Password = "fila-de-teste";

    private readonly int _brokerPort = FreeTcpPort();
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private readonly RabbitMqContainer _broker;

    private BrokerApiFactory()
    {
        _broker = new RabbitMqBuilder("rabbitmq:3.13-alpine")
            .WithUsername(User)
            .WithPassword(Password)
            .WithPortBinding(_brokerPort, 5672)
            .Build();
    }

    /// <summary>Starts the databases and the API; with <paramref name="brokerUp"/> false the API starts with the broker down.</summary>
    public static async Task<BrokerApiFactory> StartAsync(bool brokerUp)
    {
        var factory = new BrokerApiFactory();
        await factory._postgres.StartAsync();
        await factory._broker.StartAsync();
        await factory.BindTestQueueAsync(); // before any event, so nothing reaches an exchange without a queue
        if (!brokerUp) await factory._broker.StopAsync();
        _ = factory.Server;
        return factory;
    }

    public Task StopBrokerAsync() => _broker.StopAsync();

    public Task StartBrokerAsync() => _broker.StartAsync();

    public ApiClient CreateApiClient() =>
        new(CreateClient(), Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);

    public async Task<JsonElement> GetHealthDetailsAsync()
    {
        var body = await CreateClient().GetStringAsync("/api/health/details");
        return JsonDocument.Parse(body).RootElement.Clone();
    }

    /// <summary>Waits (up to <paramref name="timeout"/>) until <paramref name="count"/> events reached the exchange.</summary>
    public async Task<List<(string RoutingKey, string Message)>> ReceiveEventsAsync(int count, TimeSpan? timeout = null)
    {
        var received = new List<(string, string)>();
        await Eventually(async () =>
        {
            await using var connection = await ConnectAsync();
            await using var channel = await connection.CreateChannelAsync();
            while (received.Count < count && await channel.BasicGetAsync(TestQueue, autoAck: true) is { } message)
                received.Add((message.RoutingKey, Encoding.UTF8.GetString(message.Body.Span)));
            return received.Count >= count;
        }, timeout ?? TimeSpan.FromSeconds(30), $"esperava {count} evento(s), chegaram {received.Count}");
        return received;
    }

    /// <summary>Polls <paramref name="condition"/> until it holds; transient errors count as "not yet".</summary>
    public static async Task Eventually(Func<Task<bool>> condition, TimeSpan timeout, string because)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (true)
        {
            try { if (await condition()) return; }
            catch (Exception) when (DateTime.UtcNow < deadline) { }
            if (DateTime.UtcNow >= deadline) Assert.Fail($"Prazo de {timeout.TotalSeconds}s esgotado: {because}");
            await Task.Delay(250);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Postgres", _postgres.GetConnectionString());
        builder.UseSetting("OTEL_EXPORTER_OTLP_ENDPOINT", "");
        builder.UseSetting("RabbitMq:Host", "127.0.0.1");
        builder.UseSetting("RabbitMq:Port", _brokerPort.ToString());
        builder.UseSetting("RabbitMq:Username", User);
        builder.UseSetting("RabbitMq:Password", Password);
        builder.UseSetting("RabbitMq:RetryDelaySeconds", "1");
        builder.UseSetting("RabbitMq:PublishTimeoutSeconds", "2");
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _broker.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private async Task BindTestQueueAsync()
    {
        await using var connection = await ConnectAsync();
        await using var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(RabbitMqEventDispatcher.ExchangeName, ExchangeType.Topic, durable: true);
        await channel.QueueDeclareAsync(TestQueue, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(TestQueue, RabbitMqEventDispatcher.ExchangeName, "order.#");
    }

    private Task<IConnection> ConnectAsync() => new ConnectionFactory
    {
        HostName = "127.0.0.1", Port = _brokerPort, UserName = User, Password = Password,
    }.CreateConnectionAsync();

    /// <summary>
    /// A free port below Linux's ephemeral range (32768+), where Docker picks the random host ports of the other test
    /// containers: a port taken from the ephemeral range could be handed to another container before ours binds it.
    /// </summary>
    private static int FreeTcpPort()
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var port = Random.Shared.Next(20000, 30000);
            try
            {
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listener.Stop();
                return port;
            }
            catch (SocketException) { }
        }
        throw new InvalidOperationException("No free port between 20000 and 30000 for the test broker.");
    }
}
