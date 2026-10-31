using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoftwareHamgueria.Application.Jobs;

namespace SoftwareHamgueria.Infrastructure.Jobs;

/// <summary>
/// The job queue (ADR 0029): Hangfire on the app's own Postgres (schema <c>hangfire</c>), with retries, schedules and a
/// dashboard. Recurring jobs are declared in <see cref="RecurringJobs"/> and registered at startup.
/// </summary>
public static class JobsRegistration
{
    public static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddHangfire((sp, config) => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                o => o.UseNpgsqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString("Postgres")),
                new PostgreSqlStorageOptions { SchemaName = "hangfire", PrepareSchemaIfNecessary = true }));
        services.AddHangfireServer(o => o.WorkerCount = 2);
        services.AddHostedService<RecurringJobs>();
        return services;
    }
}

/// <summary>Declares the recurring jobs (idempotent: AddOrUpdate on every start).</summary>
public sealed class RecurringJobs(IRecurringJobManager jobs) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        jobs.AddOrUpdate<AuditRetentionJob>(
            AuditRetentionJob.Id, job => job.RunAsync(), Cron.Daily(3), new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
