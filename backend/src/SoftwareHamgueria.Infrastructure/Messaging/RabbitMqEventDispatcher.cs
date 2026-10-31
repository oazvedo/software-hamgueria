using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using SoftwareHamgueria.Application.Telemetry;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.Infrastructure.Data;

namespace SoftwareHamgueria.Infrastructure.Messaging;

/// <summary>
/// Sends the outbox to RabbitMQ in the background (ADR 0031). Reads unsent messages oldest first, in batches locked with
/// <c>FOR UPDATE SKIP LOCKED</c> (two API instances never send the same row), publishes with confirms and marks each
/// one sent. With the broker down, messages simply stay in the table, across restarts, until it comes back.
/// </summary>
public sealed class RabbitMqEventDispatcher : BackgroundService
{
    public const string ExchangeName = "hamgueria.orders";
    private const int BatchSize = 50;

    private readonly IServiceScopeFactory _scopes;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqEventDispatcher> _logger;
    private readonly TimeSpan _retryDelay;
    private readonly TimeSpan _publishTimeout;
    private readonly TimeSpan _pollInterval;
    private readonly Func<int> _readPending; // kept alive here: the telemetry gauge holds it only weakly
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _outageLogged;
    private volatile int _pending;

    public RabbitMqEventDispatcher(IServiceScopeFactory scopes, IConfiguration configuration, ILogger<RabbitMqEventDispatcher> logger)
    {
        _scopes = scopes;
        _configuration = configuration;
        _logger = logger;
        _retryDelay = Seconds("RabbitMq:RetryDelaySeconds", 5);
        _publishTimeout = Seconds("RabbitMq:PublishTimeoutSeconds", 5);
        _pollInterval = Seconds("Outbox:PollSeconds", 0.5);
        _readPending = () => _pending;
        HamgueriaTelemetry.TrackPendingEvents(_readPending);
    }

    public bool IsConnected => _connection?.IsOpen == true && _channel?.IsOpen == true;

    /// <summary>Unsent outbox messages at the last look (for the health details and the gauge).</summary>
    public int PendingCount => _pending;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshPendingAsync(stoppingToken);
                if (!IsConnected) await ConnectAsync(stoppingToken);
                var handled = await SendBatchAsync(stoppingToken);
                if (handled == 0) await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!_outageLogged)
                {
                    _logger.LogWarning(ex, "Order events not sent; they wait in the outbox ({Pending} pending)", _pending);
                    _outageLogged = true;
                }
                await DisconnectAsync();
                try { await Task.Delay(_retryDelay, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await DisconnectAsync();
    }

    private async Task RefreshPendingAsync(CancellationToken ct)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _pending = await db.OutboxMessages.CountAsync(m => m.SentAt == null, ct);
    }

    /// <summary>Sends one locked batch; progress is saved even if the broker drops mid-batch. Returns rows handled.</summary>
    private async Task<int> SendBatchAsync(CancellationToken ct)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var batch = await db.OutboxMessages
            .FromSqlRaw($"""
                SELECT * FROM "OutboxMessages" WHERE "SentAt" IS NULL
                ORDER BY "CreatedAt", "Id" LIMIT {BatchSize} FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(ct);

        var handled = 0;
        try
        {
            foreach (var message in batch)
            {
                await PublishAsync(message, ct);
                handled++;
            }
        }
        finally
        {
            // Whatever was confirmed (or refused for good) is recorded, even when the batch stops halfway.
            await db.SaveChangesAsync(CancellationToken.None);
            await transaction.CommitAsync(CancellationToken.None);
            _pending = Math.Max(0, _pending - handled);
        }
        return handled;
    }

    private async Task PublishAsync(OutboxMessage message, CancellationToken ct)
    {
        using var activity = HamgueriaTelemetry.Source.StartActivity("publish order event", System.Diagnostics.ActivityKind.Producer);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", ExchangeName);
        activity?.SetTag("messaging.rabbitmq.destination.routing_key", message.RoutingKey);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(_publishTimeout);
        try
        {
            await _channel!.BasicPublishAsync(
                ExchangeName, message.RoutingKey, mandatory: false,
                new BasicProperties { Persistent = true, MessageId = message.Id.ToString() },
                System.Text.Encoding.UTF8.GetBytes(message.Payload), timeout.Token);
            message.MarkSent();
            HamgueriaTelemetry.OrderEventsPublished.Add(1);
        }
        catch (PublishException ex)
        {
            // Refused for good (nack): keep the row for inspection but stop retrying it, or it blocks the rest.
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, "nack");
            message.MarkFailed("nack: " + ex.Message);
            message.MarkSent();
            HamgueriaTelemetry.OrderEventsDropped.Add(1, new KeyValuePair<string, object?>("reason", "nack"));
            _logger.LogError(ex, "RabbitMQ refused {RoutingKey} for order {OrderId}; outbox row {Id} kept as failed",
                message.RoutingKey, message.Payload, message.Id);
        }
        catch (Exception ex)
        {
            // Connection lost or timeout: the row stays unsent and the loop reconnects; the span says it failed.
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.GetType().Name);
            message.MarkFailed(ex.GetType().Name);
            throw;
        }
    }

    private async Task ConnectAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMq:Port"] ?? "5672"),
            UserName = _configuration["RabbitMq:Username"] ?? "guest",
            Password = _configuration["RabbitMq:Password"] ?? "guest",
            // This loop owns reconnection; the client's own recovery would hide the outage from IsConnected.
            AutomaticRecoveryEnabled = false,
            TopologyRecoveryEnabled = false,
        };

        _connection = await factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true), ct);
        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: ct);

        _logger.LogInformation("RabbitMQ connected to exchange '{Exchange}' ({Pending} events pending)", ExchangeName, _pending);
        _outageLogged = false;
    }

    private async Task DisconnectAsync()
    {
        var channel = _channel;
        var connection = _connection;
        _channel = null;
        _connection = null;
        try { if (channel is not null) await channel.DisposeAsync(); } catch { /* already broken */ }
        try { if (connection is not null) await connection.DisposeAsync(); } catch { /* already broken */ }
    }

    private TimeSpan Seconds(string key, double fallback) =>
        TimeSpan.FromSeconds(double.TryParse(_configuration[key], System.Globalization.CultureInfo.InvariantCulture, out var s) && s > 0 ? s : fallback);
}
