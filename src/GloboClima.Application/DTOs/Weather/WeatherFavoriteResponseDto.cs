namespace GloboClima.Application.DTOs.Weather;

public class WeatherFavoriteResponseDto
{
    public Guid Id { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime AddedAt { get; set; }
    public WeatherInfoDto? CurrentWeather { get; set; }
}

public class WeatherInfoDto
{
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
}