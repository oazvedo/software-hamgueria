using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoftwareHamgueria.Application.Audit;
using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.Infrastructure.Audit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Infrastructure.Data;
using SoftwareHamgueria.Infrastructure.Jobs;
using SoftwareHamgueria.Infrastructure.Messaging;
using SoftwareHamgueria.Infrastructure.Repositories;

namespace SoftwareHamgueria.Infrastructure;

public static class HealthTags
{
    public const string Database = "db";
    public const string Queue = "queue";
}

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // The API replaces this with the request's author and screen (ADR 0027); without a request it is "sistema".
        services.TryAddScoped<IAuditContext, SystemAuditContext>();
        services.AddDbContext<AppDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"))
                .AddInterceptors(new AuditContextInterceptor(sp.GetRequiredService<IAuditContext>())));

        // /api/health looks only at "db"; /api/health/details shows both (spec 005).
        // Transactional outbox (ADR 0031): events are staged in the request's DbContext and sent in the background.
        services.AddScoped<IOrderEventPublisher, OutboxOrderEventPublisher>();
        services.AddSingleton<RabbitMqEventDispatcher>();
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMqEventDispatcher>());
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(name: "database", tags: [HealthTags.Database])
            .AddCheck<RabbitMqHealthCheck>("queue", failureStatus: HealthStatus.Degraded, tags: [HealthTags.Queue]);

        services.AddScoped<IDishRepository, DishRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IComandaRepository, ComandaRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        services.AddJobs();

        // Order status is now managed manually from the kitchen screen (/producao),
        // so the automatic RabbitMQ status consumer is disabled. Re-enable to let an
        // external system drive `order.status.#` transitions again.
        // services.AddHostedService<OrderStatusConsumer>();

        return services;
    }
}
