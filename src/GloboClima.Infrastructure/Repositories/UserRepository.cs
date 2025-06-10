using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;

namespace GloboClima.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DynamoDBContext _context;
    private const string TableName = "GloboClima-Users";

    public UserRepository(DynamoDbContext dynamoDbContext)
    {
        _context = dynamoDbContext.Context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.LoadAsync<User>(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var config = new DynamoDBOperationConfig
        {
            OverrideTableName = TableName,
            IndexName = "email-index"
        };

        var search = _context.QueryAsync<User>(email, config);
        var users = await search.GetRemainingAsync();
        return users.FirstOrDefault();
    }

    public async Task<User> CreateAsync(User user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        await _context.SaveAsync(user);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveAsync(user);
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _context.DeleteAsync<User>(id);
    }

    public async Task<bool> ExistsAsync(string email)
    {
        var user = await GetByEmailAsync(email);
        return user != null;
    }
}