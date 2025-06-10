using System.ComponentModel.DataAnnotations;

namespace GloboClima.Application.DTOs.Weather;

public class WeatherFavoriteRequestDto
{
    [Required(ErrorMessage = "Nome da cidade é obrigatório")]
    public string CityName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Código do país é obrigatório")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Código do país deve ter 2 caracteres")]
    public string CountryCode { get; set; } = string.Empty;
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}