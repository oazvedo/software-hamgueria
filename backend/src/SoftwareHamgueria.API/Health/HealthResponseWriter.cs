using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SoftwareHamgueria.API.Health;

/// <summary>
/// Body of <c>/api/health/details</c> (spec 005, RN-08): overall status, each dependency and the events waiting.
/// Never the check descriptions or exceptions, which can carry hosts and users.
/// </summary>
public static class HealthResponseWriter
{
    public static Task WriteDetails(HttpContext context, HealthReport report)
    {
        var body = new
        {
            status = report.Status.ToString(),
            database = State(report, "database"),
            queue = State(report, "queue"),
            pendingEvents = report.Entries.TryGetValue("queue", out var queue) && queue.Data.TryGetValue("pendingEvents", out var n) ? n : 0,
        };
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }

    private static string State(HealthReport report, string check) =>
        report.Entries.TryGetValue(check, out var entry) && entry.Status == HealthStatus.Healthy
            ? nameof(HealthStatus.Healthy)
            : nameof(HealthStatus.Unhealthy);
}
