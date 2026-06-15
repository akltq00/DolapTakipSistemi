namespace DolapTakipSistemi.Application.Interfaces;

public interface ISifreHashService
{
    string Hashle(string sifre);

    bool Dogrula(string sifre, string sifreHash);
}
