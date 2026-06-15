using DolapTakipSistemi.Application.Interfaces;
using DolapTakipSistemi.Infrastructure.Persistence;
using DolapTakipSistemi.Infrastructure.Repositories;
using DolapTakipSistemi.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DolapTakipSistemi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IDolapRepository, EfDolapRepository>();
        services.AddScoped<ISifreHashService, Pbkdf2SifreHashService>();

        return services;
    }
}
