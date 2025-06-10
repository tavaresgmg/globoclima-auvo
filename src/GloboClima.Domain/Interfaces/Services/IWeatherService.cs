using GloboClima.Domain.Models.Weather;

namespace GloboClima.Domain.Interfaces.Services;

public interface IWeatherService
{
    Task<WeatherResponse?> GetWeatherByCityAsync(string cityName);
    Task<WeatherResponse?> GetWeatherByCoordinatesAsync(double lat, double lon);
}