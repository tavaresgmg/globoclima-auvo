using System.Net.Http.Json;
using GloboClima.Domain.Interfaces.Services;
using GloboClima.Domain.Models.Weather;
using Microsoft.Extensions.Configuration;

namespace GloboClima.Infrastructure.Services;

public class WeatherApiService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.weatherapi.com/v1";

    public WeatherApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["WeatherApiKey"] ?? "demo";
        Console.WriteLine($"WeatherApiService initialized with API key: {(_apiKey == "demo" ? "DEMO MODE" : "REAL KEY")}");
    }

    public async Task<WeatherResponse?> GetWeatherByCityAsync(string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            return null;

        try
        {
            // If no API key, return mock data for development
            if (_apiKey == "demo" || string.IsNullOrEmpty(_apiKey))
            {
                return GetMockWeatherData(cityName);
            }

            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/current.json?key={_apiKey}&q={cityName}&aqi=no");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw JSON from WeatherAPI: {json}");
                var data = System.Text.Json.JsonSerializer.Deserialize<WeatherApiResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Console.WriteLine($"Deserialized data - Location null: {data?.Location == null}, Current null: {data?.Current == null}");
                return MapToWeatherResponse(data);
            }

            return null;
        }
        catch
        {
            // Return mock data on error
            return GetMockWeatherData(cityName);
        }
    }

    public async Task<WeatherResponse?> GetWeatherByCoordinatesAsync(double lat, double lon)
    {
        try
        {
            if (_apiKey == "demo" || string.IsNullOrEmpty(_apiKey))
            {
                return GetMockWeatherData($"{lat},{lon}");
            }

            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/current.json?key={_apiKey}&q={lat},{lon}&aqi=no");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<WeatherApiResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return MapToWeatherResponse(data);
            }

            return null;
        }
        catch
        {
            return GetMockWeatherData($"{lat},{lon}");
        }
    }

    private WeatherResponse GetMockWeatherData(string location)
    {
        var random = new Random();
        var temps = new[] { 15.5, 20.2, 25.8, 30.1, 18.7, 22.3, 28.5 };
        var conditions = new[] { "Clear", "Cloudy", "Partly Cloudy", "Rainy", "Sunny" };
        var icons = new[] { "01d", "02d", "03d", "09d", "01d" };
        
        return new WeatherResponse
        {
            Name = location,
            Main = new Main
            {
                Temp = temps[random.Next(temps.Length)],
                FeelsLike = temps[random.Next(temps.Length)],
                TempMin = temps[random.Next(temps.Length)] - 5,
                TempMax = temps[random.Next(temps.Length)] + 5,
                Pressure = 1013,
                Humidity = random.Next(40, 80)
            },
            Weather = new List<WeatherInfo>
            {
                new WeatherInfo
                {
                    Id = random.Next(800, 804),
                    Main = conditions[random.Next(conditions.Length)],
                    Description = conditions[random.Next(conditions.Length)].ToLower(),
                    Icon = icons[random.Next(icons.Length)]
                }
            },
            Wind = new Wind
            {
                Speed = random.Next(5, 20),
                Deg = random.Next(0, 360)
            },
            Clouds = new Clouds
            {
                All = random.Next(0, 100)
            },
            Sys = new Sys
            {
                Country = "BR",
                Sunrise = DateTimeOffset.UtcNow.AddHours(-12).ToUnixTimeSeconds(),
                Sunset = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            },
            Coord = new Coord
            {
                Lat = -15.7801,
                Lon = -47.9292
            },
            Timezone = -10800,
            Id = random.Next(1000000, 9999999),
            Cod = 200
        };
    }

    private WeatherResponse MapToWeatherResponse(WeatherApiResponse? data)
    {
        if (data == null) 
        {
            Console.WriteLine("WeatherApiResponse data is null, returning mock data");
            return GetMockWeatherData("Unknown");
        }
        
        Console.WriteLine($"Raw WeatherAPI data - Location: {data.Location?.Name}, Current: {data.Current != null}");
        if (data.Current != null)
        {
            Console.WriteLine($"Current data - TempC: {data.Current.TempC}, FeelslikeC: {data.Current.FeelslikeC}, Humidity: {data.Current.Humidity}");
        }

        return new WeatherResponse
        {
            Name = data.Location?.Name ?? "Unknown",
            Main = new Main
            {
                Temp = data.Current?.TempC ?? 0,
                FeelsLike = data.Current?.FeelslikeC ?? 0,
                TempMin = (data.Current?.TempC ?? 0) - 2,
                TempMax = (data.Current?.TempC ?? 0) + 2,
                Pressure = (int)(data.Current?.PressureMb ?? 1013),
                Humidity = data.Current?.Humidity ?? 0
            },
            Weather = new List<WeatherInfo>
            {
                new WeatherInfo
                {
                    Id = 800,
                    Main = TranslationService.TranslateWeatherCondition(data.Current?.Condition?.Text ?? "Unknown"),
                    Description = TranslationService.TranslateWeatherCondition(data.Current?.Condition?.Text ?? "unknown"),
                    Icon = MapIcon(data.Current?.Condition?.Code ?? 1000)
                }
            },
            Wind = new Wind
            {
                Speed = (data.Current?.WindKph ?? 0) / 3.6, // Convert to m/s
                Deg = data.Current?.WindDegree ?? 0
            },
            Clouds = new Clouds
            {
                All = data.Current?.Cloud ?? 0
            },
            Sys = new Sys
            {
                Country = TranslationService.TranslateCountryName(data.Location?.Country ?? "Unknown"),
                Sunrise = DateTimeOffset.UtcNow.AddHours(-12).ToUnixTimeSeconds(),
                Sunset = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            },
            Coord = new Coord
            {
                Lat = data.Location?.Lat ?? 0,
                Lon = data.Location?.Lon ?? 0
            },
            Timezone = -10800,
            Id = new Random().Next(1000000, 9999999),
            Cod = 200
        };
    }

    private string MapIcon(int code)
    {
        return code switch
        {
            1000 => "01d", // Clear
            1003 => "02d", // Partly cloudy
            1006 => "03d", // Cloudy
            1009 => "04d", // Overcast
            >= 1180 and <= 1201 => "09d", // Rain
            >= 1210 and <= 1225 => "13d", // Snow
            >= 1273 and <= 1282 => "11d", // Thunder
            _ => "01d"
        };
    }
}

// WeatherAPI response models
public class WeatherApiResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("location")]
    public Location? Location { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("current")]
    public Current? Current { get; set; }
}

public class Location
{
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("region")]
    public string? Region { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("country")]
    public string? Country { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("lat")]
    public double Lat { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("lon")]
    public double Lon { get; set; }
}

public class Current
{
    [System.Text.Json.Serialization.JsonPropertyName("temp_c")]
    public double TempC { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("feelslike_c")]
    public double FeelslikeC { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("wind_degree")]
    public int WindDegree { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("pressure_mb")]
    public double PressureMb { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("humidity")]
    public int Humidity { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("cloud")]
    public int Cloud { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("condition")]
    public Condition? Condition { get; set; }
}

public class Condition
{
    [System.Text.Json.Serialization.JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("icon")]
    public string? Icon { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public int Code { get; set; }
}