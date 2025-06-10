using Amazon.DynamoDBv2.DataModel;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Infrastructure.Models;

namespace GloboClima.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DynamoDBContext _context;

    public UserRepository(DynamoDbContext dynamoDbContext)
    {
        _context = dynamoDbContext.Context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var dynamoUser = await _context.LoadAsync<DynamoUser>(id.ToString());
        return dynamoUser == null ? null : MapToDomain(dynamoUser);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var search = _context.QueryAsync<DynamoUser>(email, new DynamoDBOperationConfig
        {
            IndexName = "email-index"
        });
        var dynamoUsers = await search.GetRemainingAsync();
        var dynamoUser = dynamoUsers.FirstOrDefault();
        return dynamoUser == null ? null : MapToDomain(dynamoUser);
    }

    public async Task<User> CreateAsync(User user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        
        var dynamoUser = MapToDynamo(user);
        await _context.SaveAsync(dynamoUser);
        
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        
        var dynamoUser = MapToDynamo(user);
        await _context.SaveAsync(dynamoUser);
        
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _context.DeleteAsync<DynamoUser>(id.ToString());
    }

    public async Task<bool> ExistsAsync(string email)
    {
        var user = await GetByEmailAsync(email);
        return user != null;
    }

    private static User MapToDomain(DynamoUser dynamoUser)
    {
        return new User
        {
            Id = Guid.Parse(dynamoUser.Id),
            Email = dynamoUser.Email,
            PasswordHash = dynamoUser.PasswordHash,
            FirstName = dynamoUser.FirstName,
            LastName = dynamoUser.LastName,
            CreatedAt = dynamoUser.CreatedAt,
            UpdatedAt = dynamoUser.UpdatedAt,
            IsActive = dynamoUser.IsActive
        };
    }

    private static DynamoUser MapToDynamo(User user)
    {
        return new DynamoUser
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsActive = user.IsActive
        };
    }
}