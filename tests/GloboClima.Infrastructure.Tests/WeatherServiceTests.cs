using FluentAssertions;
using GloboClima.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace GloboClima.Infrastructure.Tests;

public class WeatherServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly IConfiguration _configuration;
    private readonly WeatherService _weatherService;

    public WeatherServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://api.weatherapi.com/v1/")
        };

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var inMemorySettings = new Dictionary<string, string>
        {
            {"WeatherAPI:ApiKey", "test-api-key"},
            {"WeatherApi:BaseUrl", "http://api.weatherapi.com/v1"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _weatherService = new WeatherService(httpClient, _configuration);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_Should_Return_Weather_Data()
    {
        // Arrange
        var city = "London";
        var mockResponse = new
        {
            location = new
            {
                name = "London",
                country = "United Kingdom",
                lat = 51.52,
                lon = -0.11
            },
            current = new
            {
                temp_c = 15.0,
                temp_f = 59.0,
                condition = new
                {
                    text = "Partly cloudy",
                    icon = "//cdn.weatherapi.com/weather/64x64/day/116.png"
                },
                humidity = 72,
                pressure_mb = 1019.0,
                wind_kph = 13.0,
                wind_dir = "WSW",
                uv = 4.0
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _weatherService.GetWeatherByCityAsync(city);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("London");
        result.Sys.Country.Should().Be("United Kingdom");
        result.Main.Temp.Should().Be(15.0);
        result.Weather.Should().NotBeEmpty();
        result.Weather.First().Description.Should().Be("Partly cloudy");
        result.Main.Humidity.Should().Be(72);
        result.Main.Pressure.Should().Be(1019);
        result.Wind.Speed.Should().Be(13.0);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_Should_Handle_NotFound_Response()
    {
        // Arrange
        var city = "InvalidCity";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("{\"error\":{\"message\":\"City not found\"}}")
            });

        // Act
        var act = async () => await _weatherService.GetWeatherByCityAsync(city);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetWeatherByCoordinatesAsync_Should_Return_Weather_Data()
    {
        // Arrange
        var latitude = 51.52;
        var longitude = -0.11;
        var mockResponse = new
        {
            location = new
            {
                name = "London",
                country = "United Kingdom",
                lat = latitude,
                lon = longitude
            },
            current = new
            {
                temp_c = 15.0,
                temp_f = 59.0,
                condition = new
                {
                    text = "Partly cloudy",
                    icon = "//cdn.weatherapi.com/weather/64x64/day/116.png"
                },
                humidity = 72,
                pressure_mb = 1019.0,
                wind_kph = 13.0,
                wind_dir = "WSW",
                uv = 4.0
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains($"q={latitude},{longitude}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _weatherService.GetWeatherByCoordinatesAsync(latitude, longitude);

        // Assert
        result.Should().NotBeNull();
        result.Coord.Lat.Should().Be(latitude);
        result.Coord.Lon.Should().Be(longitude);
        result.Name.Should().Be("London");
        result.Sys.Country.Should().Be("United Kingdom");
    }

    [Fact]
    public async Task GetWeatherByCityAsync_Should_Include_ApiKey_In_Request()
    {
        // Arrange
        var city = "London";
        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"location\":{\"name\":\"London\"},\"current\":{\"temp_c\":15}}")
            });

        // Act
        await _weatherService.GetWeatherByCityAsync(city);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.Query.Should().Contain("key=test-api-key");
        capturedRequest.RequestUri.Query.Should().Contain($"q={city}");
    }

    [Fact]
    public async Task GetWeatherByCityAsync_Should_Handle_Network_Error()
    {
        // Arrange
        var city = "London";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var act = async () => await _weatherService.GetWeatherByCityAsync(city);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Network error");
    }
}