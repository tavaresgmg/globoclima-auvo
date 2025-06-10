using Amazon.DynamoDBv2.DataModel;

namespace GloboClima.Domain.Entities;

[DynamoDBTable("GloboClima-Users")]
public class User
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }
    
    [DynamoDBGlobalSecondaryIndexHashKey("email-index")]
    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    [DynamoDBIgnore]
    public List<WeatherFavorite> WeatherFavorites { get; set; } = new();
    
    [DynamoDBIgnore]
    public List<CountryFavorite> CountryFavorites { get; set; } = new();
}