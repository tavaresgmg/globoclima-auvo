using GloboClima.Application.DTOs.Weather;
using GloboClima.Domain.Models.Weather;

namespace GloboClima.Application.Interfaces;

public interface IWeatherUseCase
{
    Task<WeatherResponse?> GetWeatherByCityAsync(string cityName);
    Task<List<WeatherFavoriteResponseDto>> GetFavoritesAsync(Guid userId);
    Task<WeatherFavoriteResponseDto?> AddFavoriteAsync(Guid userId, WeatherFavoriteRequestDto request);
    Task<bool> RemoveFavoriteAsync(Guid userId, Guid favoriteId);
}