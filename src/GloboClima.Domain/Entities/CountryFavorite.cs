namespace GloboClima.Domain.Entities;

public class CountryFavorite
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    
    public User User { get; set; } = null!;
}