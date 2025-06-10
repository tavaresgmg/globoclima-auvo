using FluentAssertions;
using GloboClima.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace GloboClima.Infrastructure.Tests;

public class WeatherApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly WeatherApiService _weatherApiService;

    public WeatherApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _configurationMock = new Mock<IConfiguration>();
        
        _configurationMock.Setup(x => x["WeatherApiKey"]).Returns("test-api-key");
        
        _weatherApiService = new WeatherApiService(_httpClient, _configurationMock.Object);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithValidCity_ShouldReturnWeatherResponse()
    {
        // Arrange
        var cityName = "São Paulo";
        var mockResponse = new
        {
            location = new
            {
                name = "São Paulo",
                region = "São Paulo",
                country = "Brazil",
                lat = -23.5505,
                lon = -46.6333
            },
            current = new
            {
                temp_c = 25.0,
                feelslike_c = 27.0,
                humidity = 65,
                wind_kph = 10.0,
                wind_degree = 180,
                pressure_mb = 1013.0,
                cloud = 20,
                condition = new
                {
                    text = "Partly cloudy",
                    code = 1003
                }
            }
        };

        var json = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _weatherApiService.GetWeatherByCityAsync(cityName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("São Paulo");
        result.Main.Temp.Should().Be(25.0);
        result.Main.FeelsLike.Should().Be(27.0);
        result.Main.Humidity.Should().Be(65);
        result.Weather.First().Description.Should().Be("parcialmente nublado"); // Translated
        result.Sys.Country.Should().Be("Brasil"); // Translated
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithNullOrEmptyCity_ShouldReturnNull()
    {
        // Act & Assert
        var result1 = await _weatherApiService.GetWeatherByCityAsync(null!);
        var result2 = await _weatherApiService.GetWeatherByCityAsync("");
        var result3 = await _weatherApiService.GetWeatherByCityAsync("   ");

        result1.Should().BeNull();
        result2.Should().BeNull();
        result3.Should().BeNull();
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithApiError_ShouldReturnNull()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _weatherApiService.GetWeatherByCityAsync("InvalidCity");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithDemoApiKey_ShouldReturnMockData()
    {
        // Arrange
        _configurationMock.Setup(x => x["WeatherApiKey"]).Returns("demo");
        var serviceWithDemoKey = new WeatherApiService(_httpClient, _configurationMock.Object);

        // Act
        var result = await serviceWithDemoKey.GetWeatherByCityAsync("TestCity");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("TestCity");
        result.Main.Temp.Should().BeGreaterThan(0);
        result.Weather.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetWeatherByCoordinatesAsync_WithValidCoordinates_ShouldReturnWeatherResponse()
    {
        // Arrange
        var lat = -23.5505;
        var lon = -46.6333;
        var mockResponse = new
        {
            location = new
            {
                name = "São Paulo",
                region = "São Paulo", 
                country = "Brazil",
                lat = lat,
                lon = lon
            },
            current = new
            {
                temp_c = 22.0,
                feelslike_c = 24.0,
                humidity = 70,
                wind_kph = 15.0,
                wind_degree = 90,
                pressure_mb = 1015.0,
                cloud = 30,
                condition = new
                {
                    text = "Sunny",
                    code = 1000
                }
            }
        };

        var json = JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _weatherApiService.GetWeatherByCoordinatesAsync(lat, lon);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("São Paulo");
        result.Main.Temp.Should().Be(22.0);
        result.Coord.Lat.Should().Be(lat);
        result.Coord.Lon.Should().Be(lon);
        result.Weather.First().Description.Should().Be("ensolarado"); // Translated
    }

    [Fact]
    public void WeatherApiService_Constructor_ShouldInitializeWithCorrectApiKey()
    {
        // Arrange
        var localConfigMock = new Mock<IConfiguration>();
        localConfigMock.Setup(x => x["WeatherApiKey"]).Returns("my-secret-key");

        // Act
        var service = new WeatherApiService(_httpClient, localConfigMock.Object);

        // Assert
        service.Should().NotBeNull();
        localConfigMock.Verify(x => x["WeatherApiKey"], Times.Once);
    }
}