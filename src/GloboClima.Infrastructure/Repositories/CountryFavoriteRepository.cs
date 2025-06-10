using Amazon.DynamoDBv2.DataModel;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;

namespace GloboClima.Infrastructure.Repositories;

public class CountryFavoriteRepository : ICountryFavoriteRepository
{
    private readonly DynamoDBContext _context;
    private const string TableName = "GloboClima-CountryFavorites";

    public CountryFavoriteRepository(DynamoDbContext dynamoDbContext)
    {
        _context = dynamoDbContext.Context;
    }

    public async Task<CountryFavorite?> GetByIdAsync(Guid id)
    {
        return await _context.LoadAsync<CountryFavorite>(id);
    }

    public async Task<List<CountryFavorite>> GetByUserIdAsync(Guid userId)
    {
        var config = new DynamoDBOperationConfig
        {
            OverrideTableName = TableName,
            IndexName = "userId-index"
        };

        var search = _context.QueryAsync<CountryFavorite>(userId.ToString(), config);
        return await search.GetRemainingAsync();
    }

    public async Task<CountryFavorite> CreateAsync(CountryFavorite favorite)
    {
        favorite.Id = Guid.NewGuid();
        favorite.AddedAt = DateTime.UtcNow;
        await _context.SaveAsync(favorite);
        return favorite;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _context.DeleteAsync<CountryFavorite>(id);
    }

    public async Task<bool> ExistsAsync(Guid userId, string countryCode)
    {
        var favorites = await GetByUserIdAsync(userId);
        return favorites.Any(f => f.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
    }
}