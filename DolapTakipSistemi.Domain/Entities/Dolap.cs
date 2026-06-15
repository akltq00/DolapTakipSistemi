using System.ComponentModel.DataAnnotations;

namespace DolapTakipSistemi.Domain.Entities;

public class Dolap
{
    public int Id { get; set; }

    public int Numara { get; set; }

    [MaxLength(120)]
    public string? OgrenciAdSoyad { get; set; }

    [MaxLength(30)]
    public string? OkulNumarasi { get; set; }

    [MaxLength(512)]
    public string? ZimmetSifreHash { get; set; }

    public DateTimeOffset? ZimmetTarihi { get; set; }

    public bool ZimmetliMi => OgrenciAdSoyad is not null && OkulNumarasi is not null;

    public void ZimmeteAl(string ad, string soyad, string okulNumarasi, string sifreHash)
    {
        if (ZimmetliMi)
        {
            throw new InvalidOperationException("Bu dolap zaten zimmetlenmis.");
        }

        OgrenciAdSoyad = $"{ad.Trim()} {soyad.Trim()}".Trim();
        OkulNumarasi = okulNumarasi.Trim();
        ZimmetSifreHash = sifreHash;
        ZimmetTarihi = DateTimeOffset.UtcNow;
    }

    public void ZimmetiKaldir()
    {
        OgrenciAdSoyad = null;
        OkulNumarasi = null;
        ZimmetSifreHash = null;
        ZimmetTarihi = null;
    }

    public void GuncelleNumara(int numara)
    {
        Numara = numara;
    }
}
