using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using GloboClima.Infrastructure.Models;
using GloboClima.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GloboClima.Infrastructure.Tests;

public class DynamoDbIntegrationTest
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly DynamoDBContext _context;
    
    public DynamoDbIntegrationTest()
    {
        // Use credentials from AWS CLI
        _dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USEast1);
        
        var config = new DynamoDBContextConfig
        {
            TableNamePrefix = "",
            ConsistentRead = true,
            SkipVersionCheck = true
        };
        
        _context = new DynamoDBContext(_dynamoDbClient, config);
    }
    
    [Fact]
    public async Task TestDynamoUser_TableAccess()
    {
        // Arrange
        var testUser = new DynamoUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        try
        {
            // Act - Test Save
            await _context.SaveAsync(testUser);
            
            // Act - Test Load
            var loadedUser = await _context.LoadAsync<DynamoUser>(testUser.Id);
            
            // Assert
            Assert.NotNull(loadedUser);
            Assert.Equal(testUser.Email, loadedUser.Email);
            
            // Cleanup
            await _context.DeleteAsync<DynamoUser>(testUser.Id);
        }
        catch (Exception ex)
        {
            throw new Exception($"DynamoDB operation failed: {ex.Message}", ex);
        }
    }
    
    [Fact]
    public async Task TestDynamoUser_QueryByEmail_UsingGSI()
    {
        // First, let's insert a test user
        var testUser = new DynamoUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = $"test-{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        try
        {
            // Save the user
            await _context.SaveAsync(testUser);
            
            // Wait a moment for GSI to update
            await Task.Delay(1000);
            
            // Try to query by email using GSI
            var queryConfig = new QueryOperationConfig
            {
                IndexName = "EmailIndex",
                KeyExpression = new Amazon.DynamoDBv2.DocumentModel.Expression
                {
                    ExpressionStatement = "Email = :email",
                    ExpressionAttributeValues = new Dictionary<string, Amazon.DynamoDBv2.DocumentModel.DynamoDBEntry>
                    {
                        { ":email", testUser.Email }
                    }
                }
            };
            
            var search = _context.FromQueryAsync<DynamoUser>(queryConfig);
            var results = await search.GetRemainingAsync();
            
            // Assert
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.Equal(testUser.Email, results[0].Email);
            
            // Cleanup
            await _context.DeleteAsync<DynamoUser>(testUser.Id);
        }
        catch (Exception ex)
        {
            throw new Exception($"GSI Query failed: {ex.Message}", ex);
        }
    }
    
    [Fact]
    public async Task TestUserRepository_GetByEmail()
    {
        // Create configuration
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "AWS:Region", "us-east-1" }
            })
            .Build();
        
        // Create DynamoDbContext
        var dynamoDbContext = new DynamoDbContext(config);
        
        // Create repository
        var repository = new UserRepository(dynamoDbContext);
        
        // Test getting a non-existent user
        var user = await repository.GetByEmailAsync("nonexistent@example.com");
        
        // Should return null, not throw exception
        Assert.Null(user);
    }
}