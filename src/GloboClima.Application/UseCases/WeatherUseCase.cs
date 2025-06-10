using GloboClima.Application.DTOs.Weather;
using GloboClima.Application.Interfaces;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Domain.Interfaces.Services;
using GloboClima.Domain.Models.Weather;

namespace GloboClima.Application.UseCases;

public class WeatherUseCase : IWeatherUseCase
{
    private readonly IWeatherService _weatherService;
    private readonly IWeatherFavoriteRepository _favoriteRepository;

    public WeatherUseCase(IWeatherService weatherService, IWeatherFavoriteRepository favoriteRepository)
    {
        _weatherService = weatherService;
        _favoriteRepository = favoriteRepository;
    }

    public async Task<WeatherResponse?> GetWeatherByCityAsync(string cityName)
    {
        return await _weatherService.GetWeatherByCityAsync(cityName);
    }

    public async Task<List<WeatherFavoriteResponseDto>> GetFavoritesAsync(Guid userId)
    {
        var favorites = await _favoriteRepository.GetByUserIdAsync(userId);
        var result = new List<WeatherFavoriteResponseDto>();

        foreach (var favorite in favorites)
        {
            var dto = new WeatherFavoriteResponseDto
            {
                Id = favorite.Id,
                CityName = favorite.CityName,
                CountryCode = favorite.CountryCode,
                Latitude = favorite.Latitude,
                Longitude = favorite.Longitude,
                AddedAt = favorite.AddedAt
            };

            var weather = await _weatherService.GetWeatherByCityAsync($"{favorite.CityName},{favorite.CountryCode}");
            if (weather != null)
            {
                dto.CurrentWeather = new WeatherInfoDto
                {
                    Temperature = weather.Main.Temp,
                    FeelsLike = weather.Main.FeelsLike,
                    Description = weather.Weather.FirstOrDefault()?.Description ?? "",
                    Icon = weather.Weather.FirstOrDefault()?.Icon ?? "",
                    Humidity = weather.Main.Humidity,
                    WindSpeed = weather.Wind.Speed
                };
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<WeatherFavoriteResponseDto?> AddFavoriteAsync(Guid userId, WeatherFavoriteRequestDto request)
    {
        var exists = await _favoriteRepository.ExistsAsync(userId, request.CityName, request.CountryCode);
        if (exists)
            return null;

        var favorite = new WeatherFavorite
        {
            UserId = userId,
            CityName = request.CityName,
            CountryCode = request.CountryCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        var created = await _favoriteRepository.CreateAsync(favorite);
        
        return new WeatherFavoriteResponseDto
        {
            Id = created.Id,
            CityName = created.CityName,
            CountryCode = created.CountryCode,
            Latitude = created.Latitude,
            Longitude = created.Longitude,
            AddedAt = created.AddedAt
        };
    }

    public async Task<bool> RemoveFavoriteAsync(Guid userId, Guid favoriteId)
    {
        var favorite = await _favoriteRepository.GetByIdAsync(favoriteId);
        if (favorite == null || favorite.UserId != userId)
            return false;

        await _favoriteRepository.DeleteAsync(favoriteId);
        return true;
    }
}