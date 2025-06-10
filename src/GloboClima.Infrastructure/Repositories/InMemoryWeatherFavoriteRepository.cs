using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using System.Collections.Concurrent;

namespace GloboClima.Infrastructure.Repositories;

public class InMemoryWeatherFavoriteRepository : IWeatherFavoriteRepository
{
    private readonly ConcurrentDictionary<Guid, WeatherFavorite> _favorites = new();
    private readonly ConcurrentDictionary<Guid, List<WeatherFavorite>> _favoritesByUser = new();

    public Task<WeatherFavorite> CreateAsync(WeatherFavorite favorite)
    {
        favorite.Id = Guid.NewGuid();
        favorite.AddedAt = DateTime.UtcNow;
        
        _favorites[favorite.Id] = favorite;
        
        if (!_favoritesByUser.ContainsKey(favorite.UserId))
        {
            _favoritesByUser[favorite.UserId] = new List<WeatherFavorite>();
        }
        _favoritesByUser[favorite.UserId].Add(favorite);
        
        return Task.FromResult(favorite);
    }

    public Task<WeatherFavorite?> GetByIdAsync(Guid id)
    {
        _favorites.TryGetValue(id, out var favorite);
        return Task.FromResult(favorite);
    }

    public Task<List<WeatherFavorite>> GetByUserIdAsync(Guid userId)
    {
        if (_favoritesByUser.TryGetValue(userId, out var favorites))
        {
            return Task.FromResult(favorites);
        }
        return Task.FromResult(new List<WeatherFavorite>());
    }

    public Task DeleteAsync(Guid id)
    {
        if (_favorites.TryRemove(id, out var favorite))
        {
            if (_favoritesByUser.TryGetValue(favorite.UserId, out var userFavorites))
            {
                userFavorites.Remove(favorite);
            }
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid userId, string cityName, string countryCode)
    {
        if (_favoritesByUser.TryGetValue(userId, out var favorites))
        {
            return Task.FromResult(favorites.Any(f => 
                f.CityName.Equals(cityName, StringComparison.OrdinalIgnoreCase) &&
                f.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase)));
        }
        return Task.FromResult(false);
    }
}