using DolapTakipSistemi.Application.Contracts;
using DolapTakipSistemi.Application.Interfaces;
using DolapTakipSistemi.Domain.Entities;

namespace DolapTakipSistemi.Application.Services;

public class DolapService : IDolapService
{
    private readonly IDolapRepository _dolapRepository;
    private readonly ISifreHashService _sifreHashService;

    public DolapService(IDolapRepository dolapRepository, ISifreHashService sifreHashService)
    {
        _dolapRepository = dolapRepository;
        _sifreHashService = sifreHashService;
    }

    public async Task<IReadOnlyList<DolapResponse>> ListeleAsync(CancellationToken cancellationToken)
    {
        var dolaplar = await _dolapRepository.ListeleAsync(cancellationToken);

        return dolaplar.Select(ToResponse).ToList();
    }

    public async Task<DolapResponse?> GetirAsync(int id, CancellationToken cancellationToken)
    {
        var dolap = await _dolapRepository.GetirAsync(id, cancellationToken);

        return dolap is null ? null : ToResponse(dolap);
    }

    public async Task<DolapResponse> OlusturAsync(DolapOlusturRequest request, CancellationToken cancellationToken)
    {
        if (await _dolapRepository.NumaraKullaniliyorMuAsync(request.Numara, null, cancellationToken))
        {
            throw new InvalidOperationException("Bu dolap numarasi zaten kullaniliyor.");
        }

        var dolap = new Dolap { Numara = request.Numara };

        _dolapRepository.Ekle(dolap);
        await _dolapRepository.KaydetAsync(cancellationToken);

        return ToResponse(dolap);
    }

    public async Task<DolapResponse?> GuncelleAsync(
        int id,
        DolapGuncelleRequest request,
        CancellationToken cancellationToken)
    {
        var dolap = await _dolapRepository.GetirAsync(id, cancellationToken);

        if (dolap is null)
        {
            return null;
        }

        if (await _dolapRepository.NumaraKullaniliyorMuAsync(request.Numara, id, cancellationToken))
        {
            throw new InvalidOperationException("Bu dolap numarasi zaten kullaniliyor.");
        }

        dolap.GuncelleNumara(request.Numara);
        await _dolapRepository.KaydetAsync(cancellationToken);

        return ToResponse(dolap);
    }

    public async Task<bool> SilAsync(int id, CancellationToken cancellationToken)
    {
        var dolap = await _dolapRepository.GetirAsync(id, cancellationToken);

        if (dolap is null)
        {
            return false;
        }

        _dolapRepository.Sil(dolap);
        await _dolapRepository.KaydetAsync(cancellationToken);

        return true;
    }

    public async Task<DolapResponse?> ZimmeteAlAsync(int id, ZimmeteAlRequest request, CancellationToken cancellationToken)
    {
        var dolap = await _dolapRepository.GetirAsync(id, cancellationToken);

        if (dolap is null)
        {
            return null;
        }

        var sifreHash = _sifreHashService.Hashle(request.Sifre);
        dolap.ZimmeteAl(request.Ad, request.Soyad, request.OkulNumarasi, sifreHash);
        await _dolapRepository.KaydetAsync(cancellationToken);

        return ToResponse(dolap);
    }

    public async Task<DolapResponse?> ZimmetiKaldirAsync(
        int id,
        ZimmetiKaldirRequest request,
        CancellationToken cancellationToken)
    {
        var dolap = await _dolapRepository.GetirAsync(id, cancellationToken);

        if (dolap is null)
        {
            return null;
        }

        if (!dolap.ZimmetliMi || dolap.ZimmetSifreHash is null)
        {
            throw new InvalidOperationException("Bu dolap zimmetli degil.");
        }

        if (!_sifreHashService.Dogrula(request.Sifre, dolap.ZimmetSifreHash))
        {
            throw new UnauthorizedAccessException("Sifre hatali.");
        }

        dolap.ZimmetiKaldir();
        await _dolapRepository.KaydetAsync(cancellationToken);

        return ToResponse(dolap);
    }

    private static DolapResponse ToResponse(Dolap dolap)
    {
        return new DolapResponse(
            dolap.Id,
            dolap.Numara,
            dolap.ZimmetliMi,
            dolap.OgrenciAdSoyad,
            dolap.OkulNumarasi,
            dolap.ZimmetTarihi);
    }
}
