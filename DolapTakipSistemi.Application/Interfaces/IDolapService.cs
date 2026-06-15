using DolapTakipSistemi.Application.Contracts;

namespace DolapTakipSistemi.Application.Interfaces;

public interface IDolapService
{
    Task<IReadOnlyList<DolapResponse>> ListeleAsync(CancellationToken cancellationToken);

    Task<DolapResponse?> GetirAsync(int id, CancellationToken cancellationToken);

    Task<DolapResponse> OlusturAsync(DolapOlusturRequest request, CancellationToken cancellationToken);

    Task<DolapResponse?> GuncelleAsync(int id, DolapGuncelleRequest request, CancellationToken cancellationToken);

    Task<bool> SilAsync(int id, CancellationToken cancellationToken);

    Task<DolapResponse?> ZimmeteAlAsync(int id, ZimmeteAlRequest request, CancellationToken cancellationToken);

    Task<DolapResponse?> ZimmetiKaldirAsync(
        int id,
        ZimmetiKaldirRequest request,
        CancellationToken cancellationToken);
}
