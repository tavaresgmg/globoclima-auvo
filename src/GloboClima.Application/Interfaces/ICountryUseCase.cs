using GloboClima.Application.DTOs.Country;

namespace GloboClima.Application.Interfaces;

public interface ICountryUseCase
{
    Task<List<CountryFavoriteResponseDto>> GetFavoritesAsync(Guid userId);
    Task<CountryFavoriteResponseDto?> AddFavoriteAsync(Guid userId, CountryFavoriteRequestDto request);
    Task<bool> RemoveFavoriteAsync(Guid userId, Guid favoriteId);
}