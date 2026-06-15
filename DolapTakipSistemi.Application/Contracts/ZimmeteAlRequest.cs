using System.ComponentModel.DataAnnotations;

namespace DolapTakipSistemi.Application.Contracts;

public class ZimmeteAlRequest
{
    [Required]
    [MaxLength(60)]
    public string Ad { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Soyad { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string OkulNumarasi { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    [MaxLength(20)]
    public string Sifre { get; set; } = string.Empty;
}
