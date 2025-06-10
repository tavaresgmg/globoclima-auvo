using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;

namespace GloboClima.Infrastructure.Repositories;

public class WeatherFavoriteRepository : IWeatherFavoriteRepository
{
    private readonly DynamoDBContext _context;
    private const string TableName = "GloboClima-WeatherFavorites";

    public WeatherFavoriteRepository(DynamoDbContext dynamoDbContext)
    {
        _context = dynamoDbContext.Context;
    }

    public async Task<WeatherFavorite?> GetByIdAsync(Guid id)
    {
        return await _context.LoadAsync<WeatherFavorite>(id);
    }

    public async Task<List<WeatherFavorite>> GetByUserIdAsync(Guid userId)
    {
        var config = new DynamoDBOperationConfig
        {
            OverrideTableName = TableName,
            IndexName = "userId-index"
        };

        var search = _context.QueryAsync<WeatherFavorite>(userId.ToString(), config);
        return await search.GetRemainingAsync();
    }

    public async Task<WeatherFavorite> CreateAsync(WeatherFavorite favorite)
    {
        favorite.Id = Guid.NewGuid();
        favorite.AddedAt = DateTime.UtcNow;
        await _context.SaveAsync(favorite);
        return favorite;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _context.DeleteAsync<WeatherFavorite>(id);
    }

    public async Task<bool> ExistsAsync(Guid userId, string cityName, string countryCode)
    {
        var favorites = await GetByUserIdAsync(userId);
        return favorites.Any(f => 
            f.CityName.Equals(cityName, StringComparison.OrdinalIgnoreCase) && 
            f.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
    }
}