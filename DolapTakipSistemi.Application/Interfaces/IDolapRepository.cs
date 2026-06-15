using DolapTakipSistemi.Domain.Entities;

namespace DolapTakipSistemi.Application.Interfaces;

public interface IDolapRepository
{
    Task<IReadOnlyList<Dolap>> ListeleAsync(CancellationToken cancellationToken);

    Task<Dolap?> GetirAsync(int id, CancellationToken cancellationToken);

    Task<bool> NumaraKullaniliyorMuAsync(int numara, int? haricId, CancellationToken cancellationToken);

    Task<bool> KayitVarMiAsync(CancellationToken cancellationToken);

    void Ekle(Dolap dolap);

    void EkleRange(IEnumerable<Dolap> dolaplar);

    void Sil(Dolap dolap);

    Task<int> KaydetAsync(CancellationToken cancellationToken);
}
