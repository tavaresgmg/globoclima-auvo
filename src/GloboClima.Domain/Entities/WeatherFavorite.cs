using Amazon.DynamoDBv2.DataModel;

namespace GloboClima.Domain.Entities;

[DynamoDBTable("GloboClima-WeatherFavorites")]
public class WeatherFavorite
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }
    
    [DynamoDBGlobalSecondaryIndexHashKey("userId-index")]
    public Guid UserId { get; set; }
    
    public string CityName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime AddedAt { get; set; }
    
    [DynamoDBIgnore]
    public User User { get; set; } = null!;
}