using DolapTakipSistemi.Application.Interfaces;
using DolapTakipSistemi.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DolapTakipSistemi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDolapService, DolapService>();

        return services;
    }
}
