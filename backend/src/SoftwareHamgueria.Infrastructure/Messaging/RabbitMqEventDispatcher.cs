using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.Application.Telemetry;

namespace SoftwareHamgueria.Infrastructure.Messaging;

/// <summary>
/// Sends the pending order events to RabbitMQ in the background (ADR 0025). Keeps a connection open, reconnecting
/// every <c>RabbitMq:RetryDelaySeconds</c> while the broker is down, and removes an event only after the broker
/// confirms it. Requests never wait for, or fail because of, the broker.
/// </summary>
public sealed class RabbitMqEventDispatcher(
    PendingOrderEvents pending,
    IConfiguration configuration,
    ILogger<RabbitMqEventDispatcher> logger) : BackgroundService
{
    public const string ExchangeName = "hamgueria.orders";

    private readonly TimeSpan _retryDelay = Seconds("RabbitMq:RetryDelaySeconds", configuration);
    private readonly TimeSpan _publishTimeout = Seconds("RabbitMq:PublishTimeoutSeconds", configuration);
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _outageLogged;

    public bool IsConnected => _connection?.IsOpen == true && _channel?.IsOpen == true;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!IsConnected) await ConnectAsync(stoppingToken);
                await DrainAsync(stoppingToken);
                await WaitForEventsOrRetryDelayAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!_outageLogged)
                {
                    logger.LogWarning(ex, "RabbitMQ unavailable; order events wait in memory ({Pending} pending)", pending.Count);
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
        if (pending.Count > 0)
            logger.LogWarning("Shutting down with {Pending} order events not sent; they are lost", pending.Count);
        await DisconnectAsync();
    }

    private async Task ConnectAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMq:Port"] ?? "5672"),
            UserName = configuration["RabbitMq:Username"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest",
            // This loop owns reconnection; the client's own recovery would hide the outage from IsConnected.
            AutomaticRecoveryEnabled = false,
            TopologyRecoveryEnabled = false,
        };

        _connection = await factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true), ct);
        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: ct);

        logger.LogInformation("RabbitMQ connected to exchange '{Exchange}' ({Pending} events pending)", ExchangeName, pending.Count);
        _outageLogged = false;
    }

    private async Task DrainAsync(CancellationToken ct)
    {
        while (pending.TryPeek(out var orderEvent))
        {
            using var activity = HamgueriaTelemetry.Source.StartActivity("publish order event", System.Diagnostics.ActivityKind.Producer);
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination.name", ExchangeName);
            activity?.SetTag("messaging.rabbitmq.destination.routing_key", orderEvent!.RoutingKey);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(_publishTimeout);
            try
            {
                await _channel!.BasicPublishAsync(
                    ExchangeName, orderEvent!.RoutingKey, mandatory: false,
                    new BasicProperties { Persistent = true }, System.Text.Encoding.UTF8.GetBytes(orderEvent.Message),
                    timeout.Token);
                HamgueriaTelemetry.OrderEventsPublished.Add(1);
            }
            catch (PublishException ex)
            {
                // The broker refused this event for good (nack): retrying would block every event behind it.
                activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, "nack");
                HamgueriaTelemetry.OrderEventsDropped.Add(1, new KeyValuePair<string, object?>("reason", "nack"));
                logger.LogError(ex, "RabbitMQ refused {RoutingKey} for order {OrderId}; event dropped",
                    orderEvent!.RoutingKey, orderEvent.Message);
            }

            pending.RemoveHead(orderEvent!);
        }
    }

    /// <summary>Returns when events arrive, or after the retry delay so a lost connection is noticed while idle.</summary>
    private async Task WaitForEventsOrRetryDelayAsync(CancellationToken ct)
    {
        using var idle = CancellationTokenSource.CreateLinkedTokenSource(ct);
        idle.CancelAfter(_retryDelay);
        try { await pending.WaitForEventsAsync(idle.Token); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
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

    private static TimeSpan Seconds(string key, IConfiguration configuration) =>
        TimeSpan.FromSeconds(double.TryParse(configuration[key], out var s) && s > 0 ? s : 5);
}
