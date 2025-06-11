using System.Text.Json;
using GloboClima.Domain.Interfaces.Services;
using GloboClima.Domain.Models.Weather;
using Microsoft.Extensions.Configuration;

namespace GloboClima.Infrastructure.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public WeatherService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("WEATHERAPI_KEY") ?? 
            configuration["WeatherAPI:ApiKey"] ?? 
            throw new InvalidOperationException("WeatherAPI key not configured");
        _baseUrl = configuration["WeatherApi:BaseUrl"] ?? "http://api.weatherapi.com/v1";
    }

    public async Task<WeatherResponse?> GetWeatherByCityAsync(string cityName)
    {
        try
        {
            var url = $"{_baseUrl}/current.json?key={_apiKey}&q={Uri.EscapeDataString(cityName)}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"WeatherAPI Response: {json}");
            
            var weatherApiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (weatherApiResponse == null)
                return null;

            // Convert WeatherAPI response to our domain model
            return new WeatherResponse
            {
                Name = weatherApiResponse.Location?.Name ?? cityName,
                Main = new Main
                {
                    Temp = weatherApiResponse.Current?.temp_c ?? 0,
                    FeelsLike = weatherApiResponse.Current?.feelslike_c ?? 0,
                    TempMin = weatherApiResponse.Current?.temp_c ?? 0,
                    TempMax = weatherApiResponse.Current?.temp_c ?? 0,
                    Pressure = (int)(weatherApiResponse.Current?.pressure_mb ?? 0),
                    Humidity = weatherApiResponse.Current?.humidity ?? 0
                },
                Weather = new List<WeatherInfo>
                {
                    new WeatherInfo
                    {
                        Id = 0,
                        Main = weatherApiResponse.Current?.condition?.Text ?? "Unknown",
                        Description = weatherApiResponse.Current?.condition?.Text ?? "Unknown",
                        Icon = weatherApiResponse.Current?.condition?.Icon ?? ""
                    }
                },
                Wind = new Wind
                {
                    Speed = weatherApiResponse.Current?.wind_kph ?? 0,
                    Deg = weatherApiResponse.Current?.wind_degree ?? 0
                },
                Clouds = new Clouds
                {
                    All = weatherApiResponse.Current?.cloud ?? 0
                },
                Sys = new Sys
                {
                    Country = weatherApiResponse.Location?.Country ?? "",
                    Sunrise = 0,
                    Sunset = 0
                },
                Coord = new Coord
                {
                    Lat = weatherApiResponse.Location?.Lat ?? 0,
                    Lon = weatherApiResponse.Location?.Lon ?? 0
                }
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<WeatherResponse?> GetWeatherByCoordinatesAsync(double lat, double lon)
    {
        try
        {
            var url = $"{_baseUrl}/current.json?key={_apiKey}&q={lat},{lon}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"WeatherAPI Response: {json}");
            
            var weatherApiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (weatherApiResponse == null)
                return null;

            // Convert WeatherAPI response to our domain model
            return new WeatherResponse
            {
                Name = weatherApiResponse.Location?.Name ?? "Unknown",
                Main = new Main
                {
                    Temp = weatherApiResponse.Current?.temp_c ?? 0,
                    FeelsLike = weatherApiResponse.Current?.feelslike_c ?? 0,
                    TempMin = weatherApiResponse.Current?.temp_c ?? 0,
                    TempMax = weatherApiResponse.Current?.temp_c ?? 0,
                    Pressure = (int)(weatherApiResponse.Current?.pressure_mb ?? 0),
                    Humidity = weatherApiResponse.Current?.humidity ?? 0
                },
                Weather = new List<WeatherInfo>
                {
                    new WeatherInfo
                    {
                        Id = 0,
                        Main = weatherApiResponse.Current?.condition?.Text ?? "Unknown",
                        Description = weatherApiResponse.Current?.condition?.Text ?? "Unknown",
                        Icon = weatherApiResponse.Current?.condition?.Icon ?? ""
                    }
                },
                Wind = new Wind
                {
                    Speed = weatherApiResponse.Current?.wind_kph ?? 0,
                    Deg = weatherApiResponse.Current?.wind_degree ?? 0
                },
                Clouds = new Clouds
                {
                    All = weatherApiResponse.Current?.cloud ?? 0
                },
                Sys = new Sys
                {
                    Country = weatherApiResponse.Location?.Country ?? "",
                    Sunrise = 0,
                    Sunset = 0
                },
                Coord = new Coord
                {
                    Lat = weatherApiResponse.Location?.Lat ?? 0,
                    Lon = weatherApiResponse.Location?.Lon ?? 0
                }
            };
        }
        catch
        {
            return null;
        }
    }
}

// WeatherAPI.com response models
internal class WeatherApiResponse
{
    public Location? Location { get; set; }
    public Current? Current { get; set; }
}

internal class Location
{
    public string? Name { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
}

internal class Current
{
    public double temp_c { get; set; }
    public double feelslike_c { get; set; }
    public double wind_kph { get; set; }
    public int wind_degree { get; set; }
    public double pressure_mb { get; set; }
    public int humidity { get; set; }
    public int cloud { get; set; }
    public Condition? condition { get; set; }
}

internal class Condition
{
    public string? Text { get; set; }
    public string? Icon { get; set; }
}