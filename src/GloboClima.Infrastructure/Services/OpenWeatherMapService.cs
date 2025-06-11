using System.Text.Json;
using GloboClima.Domain.Interfaces.Services;
using GloboClima.Domain.Models.Weather;
using Microsoft.Extensions.Configuration;

namespace GloboClima.Infrastructure.Services;

public class OpenWeatherMapService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IConfiguration _configuration;

    public OpenWeatherMapService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
        _configuration = configuration;
        _apiKey = Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY") ?? 
            configuration["OpenWeatherMap:ApiKey"] ?? 
            throw new InvalidOperationException("OpenWeatherMap API key not configured");
    }

    public async Task<WeatherResponse?> GetWeatherByCityAsync(string city)
    {
        var response = await _httpClient.GetAsync($"weather?q={city}&appid={_apiKey}&units=metric");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json);
        var root = data.RootElement;

        return MapToWeatherResponse(root);
    }

    public async Task<WeatherResponse?> GetWeatherByCoordinatesAsync(double latitude, double longitude)
    {
        var response = await _httpClient.GetAsync($"weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json);
        var root = data.RootElement;

        return MapToWeatherResponse(root);
    }

    private WeatherResponse MapToWeatherResponse(JsonElement root)
    {
        var weatherArray = root.GetProperty("weather");
        var weatherElement = weatherArray.EnumerateArray().FirstOrDefault();
        
        return new WeatherResponse
        {
            Name = root.GetProperty("name").GetString() ?? "",
            Coord = new Coord
            {
                Lat = root.GetProperty("coord").GetProperty("lat").GetDouble(),
                Lon = root.GetProperty("coord").GetProperty("lon").GetDouble()
            },
            Weather = new List<WeatherInfo>
            {
                new WeatherInfo
                {
                    Id = weatherElement.GetProperty("id").GetInt32(),
                    Main = weatherElement.GetProperty("main").GetString() ?? "",
                    Description = weatherElement.GetProperty("description").GetString() ?? "",
                    Icon = weatherElement.GetProperty("icon").GetString() ?? ""
                }
            },
            Main = new Main
            {
                Temp = root.GetProperty("main").GetProperty("temp").GetDouble(),
                FeelsLike = root.GetProperty("main").GetProperty("feels_like").GetDouble(),
                TempMin = root.GetProperty("main").GetProperty("temp_min").GetDouble(),
                TempMax = root.GetProperty("main").GetProperty("temp_max").GetDouble(),
                Pressure = root.GetProperty("main").GetProperty("pressure").GetInt32(),
                Humidity = root.GetProperty("main").GetProperty("humidity").GetInt32()
            },
            Wind = new Wind
            {
                Speed = root.GetProperty("wind").GetProperty("speed").GetDouble(),
                Deg = root.GetProperty("wind").GetProperty("deg").GetInt32()
            },
            Sys = new Sys
            {
                Country = root.GetProperty("sys").GetProperty("country").GetString() ?? "",
                Sunrise = root.GetProperty("sys").GetProperty("sunrise").GetInt64(),
                Sunset = root.GetProperty("sys").GetProperty("sunset").GetInt64()
            },
            Visibility = root.TryGetProperty("visibility", out var vis) ? vis.GetInt32() : 0,
            Dt = root.GetProperty("dt").GetInt64(),
            Timezone = root.GetProperty("timezone").GetInt32(),
            Id = root.GetProperty("id").GetInt32(),
            Cod = root.TryGetProperty("cod", out var cod) ? cod.GetInt32() : 200
        };
    }

}