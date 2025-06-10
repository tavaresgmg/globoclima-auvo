namespace GloboClima.Domain.Entities;

public class WeatherFavorite
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime AddedAt { get; set; }
    
    public User User { get; set; } = null!;
}