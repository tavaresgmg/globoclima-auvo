using Amazon.DynamoDBv2.DataModel;

namespace GloboClima.Domain.Entities;

[DynamoDBTable("GloboClima-CountryFavorites")]
public class CountryFavorite
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }
    
    [DynamoDBGlobalSecondaryIndexHashKey("userId-index")]
    public Guid UserId { get; set; }
    
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    
    [DynamoDBIgnore]
    public User User { get; set; } = null!;
}