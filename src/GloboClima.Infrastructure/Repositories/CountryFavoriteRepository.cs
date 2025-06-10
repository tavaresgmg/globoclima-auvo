using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Infrastructure.Models;

namespace GloboClima.Infrastructure.Repositories;

public class CountryFavoriteRepository : ICountryFavoriteRepository
{
    private readonly DynamoDBContext _context;

    public CountryFavoriteRepository(DynamoDbContext dynamoDbContext)
    {
        _context = dynamoDbContext.Context;
    }

    public async Task<CountryFavorite?> GetByIdAsync(Guid id)
    {
        var dynamoCountryFavorite = await _context.LoadAsync<DynamoCountryFavorite>(id.ToString());
        return dynamoCountryFavorite == null ? null : MapToDomain(dynamoCountryFavorite);
    }

    public async Task<List<CountryFavorite>> GetByUserIdAsync(Guid userId)
    {
        var queryConfig = new QueryConfig
        {
            IndexName = "userId-index"
        };

        var search = _context.QueryAsync<DynamoCountryFavorite>(userId.ToString(), queryConfig);
        var dynamoFavorites = await search.GetRemainingAsync();
        return dynamoFavorites.Select(MapToDomain).ToList();
    }

    public async Task<CountryFavorite> CreateAsync(CountryFavorite favorite)
    {
        favorite.Id = Guid.NewGuid();
        favorite.AddedAt = DateTime.UtcNow;
        
        var dynamoFavorite = MapToDynamo(favorite);
        await _context.SaveAsync(dynamoFavorite);
        
        return favorite;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _context.DeleteAsync<DynamoCountryFavorite>(id.ToString());
    }

    public async Task<bool> ExistsAsync(Guid userId, string countryCode)
    {
        var favorites = await GetByUserIdAsync(userId);
        return favorites.Any(f => f.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
    }

    private static CountryFavorite MapToDomain(DynamoCountryFavorite dynamoFavorite)
    {
        return new CountryFavorite
        {
            Id = Guid.Parse(dynamoFavorite.Id),
            UserId = Guid.Parse(dynamoFavorite.UserId),
            CountryCode = dynamoFavorite.CountryCode,
            CountryName = dynamoFavorite.CountryName,
            Region = dynamoFavorite.Region,
            AddedAt = dynamoFavorite.AddedAt
        };
    }

    private static DynamoCountryFavorite MapToDynamo(CountryFavorite favorite)
    {
        return new DynamoCountryFavorite
        {
            Id = favorite.Id.ToString(),
            UserId = favorite.UserId.ToString(),
            CountryCode = favorite.CountryCode,
            CountryName = favorite.CountryName,
            Region = favorite.Region,
            AddedAt = favorite.AddedAt
        };
    }
}