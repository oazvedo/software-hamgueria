using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Infrastructure.Data;
using SoftwareHamgueria.Infrastructure.Messaging;
using SoftwareHamgueria.Infrastructure.Repositories;

namespace SoftwareHamgueria.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IDishRepository, DishRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IComandaRepository, ComandaRepository>();

        services.AddSingleton<IOrderEventPublisher, RabbitMqPublisher>();

        // Order status is now managed manually from the kitchen screen (/producao),
        // so the automatic RabbitMQ status consumer is disabled. Re-enable to let an
        // external system drive `order.status.#` transitions again.
        // services.AddHostedService<OrderStatusConsumer>();

        return services;
    }
}
