using Amazon.DynamoDBv2.DataModel;

namespace GloboClima.Infrastructure.Models;

[DynamoDBTable("GloboClima-WeatherFavorites")]
public class DynamoWeatherFavorite
{
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;
    
    [DynamoDBGlobalSecondaryIndexHashKey("userId-index")]
    [DynamoDBProperty("UserId")]
    public string UserId { get; set; } = string.Empty;
    
    public string CityName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime AddedAt { get; set; }
}