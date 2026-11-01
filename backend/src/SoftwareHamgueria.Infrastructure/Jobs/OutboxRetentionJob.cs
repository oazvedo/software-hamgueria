using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftwareHamgueria.Infrastructure.Data;

namespace SoftwareHamgueria.Infrastructure.Jobs;

/// <summary>
/// Deletes outbox rows sent more than 7 days ago (ADR 0031). Unsent rows are never touched. It lives in the
/// Infrastructure because the outbox is a storage detail, not a business rule.
/// </summary>
public class OutboxRetentionJob(AppDbContext db, TimeProvider clock, ILogger<OutboxRetentionJob> logger)
{
    public const string Id = "outbox-retention";
    public static readonly TimeSpan KeepSent = TimeSpan.FromDays(7);

    public async Task<int> RunAsync()
    {
        var cutoff = clock.GetUtcNow().UtcDateTime - KeepSent;
        var deleted = await db.OutboxMessages.Where(m => m.SentAt != null && m.SentAt < cutoff).ExecuteDeleteAsync();
        logger.LogInformation("Outbox retention: deleted {Deleted} sent messages older than {Cutoff:o}", deleted, cutoff);
        return deleted;
    }
}
