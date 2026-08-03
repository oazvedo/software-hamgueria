using Microsoft.Extensions.DependencyInjection;
using SoftwareHamgueria.Application.Services;
using SoftwareHamgueria.Application.Services.Interfaces;

namespace SoftwareHamgueria.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDishService, DishService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
