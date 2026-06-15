using System.ComponentModel.DataAnnotations;

namespace DolapTakipSistemi.Application.Contracts;

public class ZimmetiKaldirRequest
{
    [Required]
    [MinLength(4)]
    [MaxLength(20)]
    public string Sifre { get; set; } = string.Empty;
}
