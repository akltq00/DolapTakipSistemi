using System.ComponentModel.DataAnnotations;

namespace DolapTakipSistemi.Application.Contracts;

public class DolapGuncelleRequest
{
    [Range(1, int.MaxValue)]
    public int Numara { get; set; }
}
