using System.Security.Cryptography;
using DolapTakipSistemi.Application.Interfaces;

namespace DolapTakipSistemi.Infrastructure.Security;

public class Pbkdf2SifreHashService : ISifreHashService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hashle(string sifre)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(sifre, salt, Iterations, Algorithm, KeySize);

        return string.Join(
            ".",
            "PBKDF2",
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    public bool Dogrula(string sifre, string sifreHash)
    {
        try
        {
            var parcalar = sifreHash.Split('.');

            if (parcalar.Length != 4 || parcalar[0] != "PBKDF2" || !int.TryParse(parcalar[1], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parcalar[2]);
            var beklenenKey = Convert.FromBase64String(parcalar[3]);
            var gelenKey = Rfc2898DeriveBytes.Pbkdf2(sifre, salt, iterations, Algorithm, beklenenKey.Length);

            return CryptographicOperations.FixedTimeEquals(gelenKey, beklenenKey);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
