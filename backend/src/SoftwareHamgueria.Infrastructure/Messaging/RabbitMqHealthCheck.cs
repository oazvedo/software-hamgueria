using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SoftwareHamgueria.Infrastructure.Data;

namespace SoftwareHamgueria.Infrastructure.Messaging;

/// <summary>
/// Queue state for <c>/api/health/details</c> (spec 005): connected or not, plus the outbox messages still unsent,
/// counted live from the table (ADR 0031) so the number is right the moment an order is written.
/// </summary>
public sealed class RabbitMqHealthCheck(RabbitMqEventDispatcher dispatcher, AppDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        int pending;
        try { pending = await db.OutboxMessages.CountAsync(m => m.SentAt == null, cancellationToken); }
        catch (Exception) { pending = dispatcher.PendingCount; } // database down: the database check already says so
        var data = new Dictionary<string, object> { ["pendingEvents"] = pending };
        return dispatcher.IsConnected
            ? HealthCheckResult.Healthy(data: data)
            : new HealthCheckResult(context.Registration.FailureStatus, data: data);
    }
}
