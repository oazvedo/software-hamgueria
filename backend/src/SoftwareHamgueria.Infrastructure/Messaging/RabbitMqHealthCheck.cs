using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SoftwareHamgueria.Infrastructure.Messaging;

/// <summary>Queue state for <c>/api/health/details</c> (spec 005): connected or not, plus the outbox messages waiting (ADR 0031).</summary>
public sealed class RabbitMqHealthCheck(RabbitMqEventDispatcher dispatcher) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object> { ["pendingEvents"] = dispatcher.PendingCount };
        return Task.FromResult(dispatcher.IsConnected
            ? HealthCheckResult.Healthy(data: data)
            : new HealthCheckResult(context.Registration.FailureStatus, data: data));
    }
}
