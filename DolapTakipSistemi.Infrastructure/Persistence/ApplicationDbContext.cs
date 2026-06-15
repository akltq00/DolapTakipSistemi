using DolapTakipSistemi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DolapTakipSistemi.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Dolap> Dolaplar => Set<Dolap>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dolap>(entity =>
        {
            entity.HasIndex(dolap => dolap.Numara).IsUnique();
            entity.Property(dolap => dolap.OgrenciAdSoyad).HasMaxLength(120);
            entity.Property(dolap => dolap.OkulNumarasi).HasMaxLength(30);
            entity.Property(dolap => dolap.ZimmetSifreHash).HasMaxLength(512);
        });
    }
}
