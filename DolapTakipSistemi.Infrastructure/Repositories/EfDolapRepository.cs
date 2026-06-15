using DolapTakipSistemi.Application.Interfaces;
using DolapTakipSistemi.Domain.Entities;
using DolapTakipSistemi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DolapTakipSistemi.Infrastructure.Repositories;

public class EfDolapRepository : IDolapRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EfDolapRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Dolap>> ListeleAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Dolaplar
            .AsNoTracking()
            .OrderBy(dolap => dolap.Numara)
            .ToListAsync(cancellationToken);
    }

    public Task<Dolap?> GetirAsync(int id, CancellationToken cancellationToken)
    {
        return _dbContext.Dolaplar.FirstOrDefaultAsync(dolap => dolap.Id == id, cancellationToken);
    }

    public Task<bool> NumaraKullaniliyorMuAsync(int numara, int? haricId, CancellationToken cancellationToken)
    {
        return _dbContext.Dolaplar.AnyAsync(
            dolap => dolap.Numara == numara && (!haricId.HasValue || dolap.Id != haricId.Value),
            cancellationToken);
    }

    public Task<bool> KayitVarMiAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Dolaplar.AnyAsync(cancellationToken);
    }

    public void Ekle(Dolap dolap)
    {
        _dbContext.Dolaplar.Add(dolap);
    }

    public void EkleRange(IEnumerable<Dolap> dolaplar)
    {
        _dbContext.Dolaplar.AddRange(dolaplar);
    }

    public void Sil(Dolap dolap)
    {
        _dbContext.Dolaplar.Remove(dolap);
    }

    public Task<int> KaydetAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
