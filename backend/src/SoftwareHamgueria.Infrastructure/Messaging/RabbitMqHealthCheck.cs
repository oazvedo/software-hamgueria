using Microsoft.Extensions.Diagnostics.HealthChecks;
using SoftwareHamgueria.Application.Messaging;

namespace SoftwareHamgueria.Infrastructure.Messaging;

/// <summary>Queue state for <c>/api/health/details</c> (spec 005): connected or not, plus the events waiting.</summary>
public sealed class RabbitMqHealthCheck(RabbitMqEventDispatcher dispatcher, PendingOrderEvents pending) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object> { ["pendingEvents"] = pending.Count };
        return Task.FromResult(dispatcher.IsConnected
            ? HealthCheckResult.Healthy(data: data)
            : new HealthCheckResult(context.Registration.FailureStatus, data: data));
    }
}
