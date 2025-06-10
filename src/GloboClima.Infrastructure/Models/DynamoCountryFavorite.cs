using Amazon.DynamoDBv2.DataModel;

namespace GloboClima.Infrastructure.Models;

[DynamoDBTable("GloboClima-CountryFavorites")]
public class DynamoCountryFavorite
{
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;
    
    [DynamoDBGlobalSecondaryIndexHashKey("userId-index")]
    [DynamoDBProperty("UserId")]
    public string UserId { get; set; } = string.Empty;
    
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}