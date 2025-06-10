using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using System.Collections.Concurrent;

namespace GloboClima.Infrastructure.Repositories;

public class InMemoryCountryFavoriteRepository : ICountryFavoriteRepository
{
    private readonly ConcurrentDictionary<Guid, CountryFavorite> _favorites = new();
    private readonly ConcurrentDictionary<Guid, List<CountryFavorite>> _favoritesByUser = new();

    public Task<CountryFavorite> CreateAsync(CountryFavorite favorite)
    {
        favorite.Id = Guid.NewGuid();
        favorite.AddedAt = DateTime.UtcNow;
        
        _favorites[favorite.Id] = favorite;
        
        if (!_favoritesByUser.ContainsKey(favorite.UserId))
        {
            _favoritesByUser[favorite.UserId] = new List<CountryFavorite>();
        }
        _favoritesByUser[favorite.UserId].Add(favorite);
        
        return Task.FromResult(favorite);
    }

    public Task<CountryFavorite?> GetByIdAsync(Guid id)
    {
        _favorites.TryGetValue(id, out var favorite);
        return Task.FromResult(favorite);
    }

    public Task<List<CountryFavorite>> GetByUserIdAsync(Guid userId)
    {
        if (_favoritesByUser.TryGetValue(userId, out var favorites))
        {
            return Task.FromResult(favorites);
        }
        return Task.FromResult(new List<CountryFavorite>());
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

    public Task<bool> ExistsAsync(Guid userId, string countryCode)
    {
        if (_favoritesByUser.TryGetValue(userId, out var favorites))
        {
            return Task.FromResult(favorites.Any(f => 
                f.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase)));
        }
        return Task.FromResult(false);
    }
}