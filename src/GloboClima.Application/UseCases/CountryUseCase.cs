using GloboClima.Application.DTOs.Country;
using GloboClima.Application.Interfaces;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Domain.Interfaces.Services;

namespace GloboClima.Application.UseCases;

public class CountryUseCase : ICountryUseCase
{
    private readonly ICountryService _countryService;
    private readonly ICountryFavoriteRepository _favoriteRepository;

    public CountryUseCase(ICountryService countryService, ICountryFavoriteRepository favoriteRepository)
    {
        _countryService = countryService;
        _favoriteRepository = favoriteRepository;
    }

    public async Task<List<CountryFavoriteResponseDto>> GetFavoritesAsync(Guid userId)
    {
        var favorites = await _favoriteRepository.GetByUserIdAsync(userId);
        var result = new List<CountryFavoriteResponseDto>();

        foreach (var favorite in favorites)
        {
            var dto = new CountryFavoriteResponseDto
            {
                Id = favorite.Id,
                CountryCode = favorite.CountryCode,
                CountryName = favorite.CountryName,
                Region = favorite.Region,
                AddedAt = favorite.AddedAt
            };

            var country = await _countryService.GetCountryByCodeAsync(favorite.CountryCode);
            if (country != null)
            {
                dto.CountryInfo = new CountryInfoDto
                {
                    Name = country.Name.Common,
                    Capital = country.Capital.FirstOrDefault() ?? "",
                    Region = country.Region,
                    Subregion = country.Subregion,
                    Population = country.Population,
                    Area = country.Area,
                    Flag = country.Flags.Svg,
                    Languages = country.Languages.Values.ToList(),
                    Currencies = country.Currencies.Values.Select(c => c.Name).ToList()
                };
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<CountryFavoriteResponseDto?> AddFavoriteAsync(Guid userId, CountryFavoriteRequestDto request)
    {
        var exists = await _favoriteRepository.ExistsAsync(userId, request.CountryCode);
        if (exists)
            return null;

        var favorite = new CountryFavorite
        {
            UserId = userId,
            CountryCode = request.CountryCode,
            CountryName = request.CountryName,
            Region = request.Region
        };

        var created = await _favoriteRepository.CreateAsync(favorite);
        
        return new CountryFavoriteResponseDto
        {
            Id = created.Id,
            CountryCode = created.CountryCode,
            CountryName = created.CountryName,
            Region = created.Region,
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