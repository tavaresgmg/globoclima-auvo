using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
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
        try
        {
            // Use scan with condition as GSI query requires non-consistent reads
            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("Email", ScanOperator.Equal, email)
            };
            var search = _context.ScanAsync<DynamoUser>(scanConditions);
            var allUsers = await search.GetRemainingAsync();
            var dynamoUser = allUsers.FirstOrDefault();
            return dynamoUser == null ? null : MapToDomain(dynamoUser);
        }
        catch (Exception ex)
        {
            // Return null on error to allow registration to proceed
            Console.WriteLine($"Error getting user by email: {ex.Message}");
            return null;
        }
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