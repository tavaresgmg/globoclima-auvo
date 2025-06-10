using GloboClima.Domain.Entities;

namespace GloboClima.Domain.Interfaces.Repositories;

public interface IWeatherFavoriteRepository
{
    Task<WeatherFavorite?> GetByIdAsync(Guid id);
    Task<List<WeatherFavorite>> GetByUserIdAsync(Guid userId);
    Task<WeatherFavorite> CreateAsync(WeatherFavorite favorite);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid userId, string cityName, string countryCode);
}