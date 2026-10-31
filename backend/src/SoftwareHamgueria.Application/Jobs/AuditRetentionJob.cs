using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoftwareHamgueria.Application.Repositories;

namespace SoftwareHamgueria.Application.Jobs;

/// <summary>
/// Deletes audit rows older than <c>Jobs:AuditRetentionDays</c> (default 180, minimum 7). Runs daily on the job queue
/// (ADR 0029); the class knows nothing about Hangfire, so it is tested like any service.
/// </summary>
public class AuditRetentionJob(
    IAuditRepository repository,
    IConfiguration configuration,
    TimeProvider clock,
    ILogger<AuditRetentionJob> logger)
{
    public const string Id = "audit-retention";
    public const int DefaultDays = 180;
    public const int MinimumDays = 7;

    public int RetentionDays =>
        int.TryParse(configuration["Jobs:AuditRetentionDays"], out var days) && days >= MinimumDays ? days : DefaultDays;

    public async Task<int> RunAsync()
    {
        var cutoff = clock.GetUtcNow().UtcDateTime.AddDays(-RetentionDays);
        var deleted = await repository.DeleteOlderThanAsync(cutoff);
        logger.LogInformation("Audit retention: deleted {Deleted} rows older than {Cutoff:o} ({Days} days)", deleted, cutoff, RetentionDays);
        return deleted;
    }
}
