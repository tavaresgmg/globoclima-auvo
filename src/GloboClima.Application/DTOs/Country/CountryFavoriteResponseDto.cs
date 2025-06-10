namespace GloboClima.Application.DTOs.Country;

public class CountryFavoriteResponseDto
{
    public Guid Id { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public CountryInfoDto? CountryInfo { get; set; }
}

public class CountryInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Capital { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Subregion { get; set; } = string.Empty;
    public long Population { get; set; }
    public double Area { get; set; }
    public string Flag { get; set; } = string.Empty;
    public List<string> Languages { get; set; } = new();
    public List<string> Currencies { get; set; } = new();
}