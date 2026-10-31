using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.Application.Services;
using SoftwareHamgueria.Application.Services.Interfaces;
using SoftwareHamgueria.Application.Validators.Order;

namespace SoftwareHamgueria.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDishService, DishService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IAuthService, AuthService>();
        // Events are queued here and sent by the Infrastructure in the background (ADR 0025).
        services.AddSingleton(sp => new PendingOrderEvents(sp.GetRequiredService<ILogger<PendingOrderEvents>>()));
        services.AddSingleton<IOrderEventPublisher, QueuedOrderEventPublisher>();
        services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();
        return services;
    }
}
