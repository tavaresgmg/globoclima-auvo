using GloboClima.Domain.Entities;

namespace GloboClima.Domain.Interfaces.Repositories;

public interface ICountryFavoriteRepository
{
    Task<CountryFavorite?> GetByIdAsync(Guid id);
    Task<List<CountryFavorite>> GetByUserIdAsync(Guid userId);
    Task<CountryFavorite> CreateAsync(CountryFavorite favorite);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid userId, string countryCode);
}