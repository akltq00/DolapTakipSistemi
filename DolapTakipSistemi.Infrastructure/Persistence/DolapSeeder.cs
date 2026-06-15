using DolapTakipSistemi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DolapTakipSistemi.Infrastructure.Persistence;

public static class DolapSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();

        if (await dbContext.Dolaplar.AnyAsync())
        {
            return;
        }

        var dolapSayisi = int.TryParse(configuration["DolapAyarlar:BaslangicDolapSayisi"], out var ayarDegeri)
            ? ayarDegeri
            : 60;
        var dolaplar = Enumerable.Range(1, dolapSayisi).Select(numara => new Dolap { Numara = numara });

        dbContext.Dolaplar.AddRange(dolaplar);
        await dbContext.SaveChangesAsync();
    }
}
