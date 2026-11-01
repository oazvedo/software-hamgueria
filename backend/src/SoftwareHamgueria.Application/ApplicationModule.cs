using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoftwareHamgueria.Application.Jobs;
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
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<AuditRetentionJob>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();
        return services;
    }
}
