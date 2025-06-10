using System.ComponentModel.DataAnnotations;

namespace GloboClima.Application.DTOs.Country;

public class CountryFavoriteRequestDto
{
    [Required(ErrorMessage = "Código do país é obrigatório")]
    public string CountryCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Nome do país é obrigatório")]
    public string CountryName { get; set; } = string.Empty;
    
    public string Region { get; set; } = string.Empty;
}