using GloboClima.Application.DTOs.Country;
using GloboClima.Domain.Models.Country;

namespace GloboClima.Application.Interfaces;

public interface ICountryUseCase
{
    Task<CountryResponse?> GetCountryByNameAsync(string name);
    Task<List<CountryFavoriteResponseDto>> GetFavoritesAsync(Guid userId);
    Task<CountryFavoriteResponseDto?> AddFavoriteAsync(Guid userId, CountryFavoriteRequestDto request);
    Task<bool> RemoveFavoriteAsync(Guid userId, Guid favoriteId);
}