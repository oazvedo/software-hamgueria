using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Infrastructure.Data;
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
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        // /api/health looks only at "db"; /api/health/details shows both (spec 005).
        services.AddSingleton<RabbitMqEventDispatcher>();
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMqEventDispatcher>());
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(name: "database", tags: [HealthTags.Database])
            .AddCheck<RabbitMqHealthCheck>("queue", failureStatus: HealthStatus.Degraded, tags: [HealthTags.Queue]);

        services.AddScoped<IDishRepository, DishRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IComandaRepository, ComandaRepository>();

        // Order status is now managed manually from the kitchen screen (/producao),
        // so the automatic RabbitMQ status consumer is disabled. Re-enable to let an
        // external system drive `order.status.#` transitions again.
        // services.AddHostedService<OrderStatusConsumer>();

        return services;
    }
}
