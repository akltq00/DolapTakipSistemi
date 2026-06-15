namespace DolapTakipSistemi.Application.Contracts;

public record DolapResponse(
    int Id,
    int Numara,
    bool ZimmetliMi,
    string? OgrenciAdSoyad,
    string? OkulNumarasi,
    DateTimeOffset? ZimmetTarihi);
