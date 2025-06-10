using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Infrastructure.Models;

namespace GloboClima.Infrastructure.Repositories;

public class WeatherFavoriteRepository : IWeatherFavoriteRepository
{
    private readonly DynamoDBContext _context;

    public WeatherFavoriteRepository(DynamoDbContext dynamoDbContext)
    {
        _context = dynamoDbContext.Context;
    }

    public async Task<WeatherFavorite?> GetByIdAsync(Guid id)
    {
        var dynamoWeatherFavorite = await _context.LoadAsync<DynamoWeatherFavorite>(id.ToString());
        return dynamoWeatherFavorite == null ? null : MapToDomain(dynamoWeatherFavorite);
    }

    public async Task<List<WeatherFavorite>> GetByUserIdAsync(Guid userId)
    {
        var queryConfig = new QueryConfig
        {
            IndexName = "userId-index"
        };

        var search = _context.QueryAsync<DynamoWeatherFavorite>(userId.ToString(), queryConfig);
        var dynamoFavorites = await search.GetRemainingAsync();
        return dynamoFavorites.Select(MapToDomain).ToList();
    }

    public async Task<WeatherFavorite> CreateAsync(WeatherFavorite favorite)
    {
        favorite.Id = Guid.NewGuid();
        favorite.AddedAt = DateTime.UtcNow;
        
        var dynamoFavorite = MapToDynamo(favorite);
        await _context.SaveAsync(dynamoFavorite);
        
        return favorite;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _context.DeleteAsync<DynamoWeatherFavorite>(id.ToString());
    }

    public async Task<bool> ExistsAsync(Guid userId, string cityName, string countryCode)
    {
        var favorites = await GetByUserIdAsync(userId);
        return favorites.Any(f => 
            f.CityName.Equals(cityName, StringComparison.OrdinalIgnoreCase) && 
            f.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
    }

    private static WeatherFavorite MapToDomain(DynamoWeatherFavorite dynamoFavorite)
    {
        return new WeatherFavorite
        {
            Id = Guid.Parse(dynamoFavorite.Id),
            UserId = Guid.Parse(dynamoFavorite.UserId),
            CityName = dynamoFavorite.CityName,
            CountryCode = dynamoFavorite.CountryCode,
            Latitude = dynamoFavorite.Latitude,
            Longitude = dynamoFavorite.Longitude,
            AddedAt = dynamoFavorite.AddedAt
        };
    }

    private static DynamoWeatherFavorite MapToDynamo(WeatherFavorite favorite)
    {
        return new DynamoWeatherFavorite
        {
            Id = favorite.Id.ToString(),
            UserId = favorite.UserId.ToString(),
            CityName = favorite.CityName,
            CountryCode = favorite.CountryCode,
            Latitude = favorite.Latitude,
            Longitude = favorite.Longitude,
            AddedAt = favorite.AddedAt
        };
    }
}