using FluentAssertions;
using GloboClima.Infrastructure.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace GloboClima.Infrastructure.Tests;

public class CountryServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly CountryService _countryService;

    public CountryServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://restcountries.com/v3.1/")
        };

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _countryService = new CountryService(_httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetCountryByCodeAsync_Should_Return_Country_Data()
    {
        // Arrange
        var countryCode = "BR";
        var mockResponse = new[]
        {
            new
            {
                name = new { common = "Brazil", official = "Federative Republic of Brazil" },
                cca2 = "BR",
                capital = new[] { "Brasília" },
                region = "Americas",
                subregion = "South America",
                population = 214326223,
                languages = new Dictionary<string, string> { { "por", "Portuguese" } },
                currencies = new
                {
                    BRL = new { name = "Brazilian real", symbol = "R$" }
                },
                flags = new { png = "https://flagcdn.com/w320/br.png" }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains($"alpha/{countryCode}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _countryService.GetCountryByCodeAsync(countryCode);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Brazil");
        result.Code.Should().Be("BR");
        result.Capital.Should().Be("Brasília");
        result.Region.Should().Be("Americas");
        result.Population.Should().Be(214326223);
        result.Languages.Should().Contain("Portuguese");
        result.Currencies.Should().Contain("Brazilian real (R$)");
        result.FlagUrl.Should().Be("https://flagcdn.com/w320/br.png");
    }

    [Fact]
    public async Task GetCountryByCodeAsync_Should_Handle_NotFound_Response()
    {
        // Arrange
        var countryCode = "XX";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("{\"status\":404,\"message\":\"Not Found\"}")
            });

        // Act
        var act = async () => await _countryService.GetCountryByCodeAsync(countryCode);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SearchCountriesByNameAsync_Should_Return_Multiple_Countries()
    {
        // Arrange
        var searchName = "united";
        var mockResponse = new[]
        {
            new
            {
                name = new { common = "United States", official = "United States of America" },
                cca2 = "US",
                capital = new[] { "Washington, D.C." },
                region = "Americas",
                population = 329484123,
                languages = new Dictionary<string, string> { { "eng", "English" } },
                currencies = new Dictionary<string, dynamic>
                {
                    { "USD", new { name = "United States dollar", symbol = "$" } }
                },
                flags = new { png = "https://flagcdn.com/w320/us.png" }
            },
            new
            {
                name = new { common = "United Kingdom", official = "United Kingdom of Great Britain and Northern Ireland" },
                cca2 = "GB",
                capital = new[] { "London" },
                region = "Europe",
                population = 67215293,
                languages = new Dictionary<string, string> { { "eng", "English" } },
                currencies = new Dictionary<string, dynamic>
                {
                    { "GBP", new { name = "British pound", symbol = "£" } }
                },
                flags = new { png = "https://flagcdn.com/w320/gb.png" }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains($"name/{searchName}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _countryService.SearchCountriesByNameAsync(searchName);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "United States");
        result.Should().Contain(c => c.Name == "United Kingdom");
    }

    [Fact]
    public async Task GetAllCountriesAsync_Should_Return_All_Countries()
    {
        // Arrange
        var mockResponse = new[]
        {
            new
            {
                name = new { common = "Brazil", official = "Federative Republic of Brazil" },
                cca2 = "BR",
                capital = new[] { "Brasília" },
                region = "Americas",
                population = 214326223,
                languages = new Dictionary<string, string> { { "por", "Portuguese" } },
                currencies = new Dictionary<string, dynamic>
                {
                    { "BRL", new { name = "Brazilian real", symbol = "R$" } }
                },
                flags = new { png = "https://flagcdn.com/w320/br.png" }
            },
            new
            {
                name = new { common = "Argentina", official = "Argentine Republic" },
                cca2 = "AR",
                capital = new[] { "Buenos Aires" },
                region = "Americas",
                population = 45605826,
                languages = new Dictionary<string, string> { { "spa", "Spanish" } },
                currencies = new Dictionary<string, dynamic>
                {
                    { "ARS", new { name = "Argentine peso", symbol = "$" } }
                },
                flags = new { png = "https://flagcdn.com/w320/ar.png" }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("all")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _countryService.GetAllCountriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Brazil" && c.Code == "BR");
        result.Should().Contain(c => c.Name == "Argentina" && c.Code == "AR");
    }

    [Fact]
    public async Task GetCountryByCodeAsync_Should_Handle_Empty_Response()
    {
        // Arrange
        var countryCode = "BR";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]")
            });

        // Act
        var act = async () => await _countryService.GetCountryByCodeAsync(countryCode);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SearchCountriesByNameAsync_Should_Handle_Network_Error()
    {
        // Arrange
        var searchName = "brazil";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var act = async () => await _countryService.SearchCountriesByNameAsync(searchName);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Network error");
    }
}