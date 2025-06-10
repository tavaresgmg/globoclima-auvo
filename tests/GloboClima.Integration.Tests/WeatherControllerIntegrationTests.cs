using FluentAssertions;
using GloboClima.Application.DTOs.Auth;
using GloboClima.Application.DTOs.Weather;
using GloboClima.Domain.Models.Weather;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GloboClima.Integration.Tests;

public class WeatherControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public WeatherControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var registerRequest = new RegisterRequestDto
        {
            Email = $"weather-test-{Guid.NewGuid()}@globoclima.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(content, _jsonOptions);
        
        return authResponse!.Token;
    }

    [Fact]
    public async Task GetWeatherByCity_WithValidCity_ShouldReturnOk()
    {
        // Arrange
        var cityName = "São Paulo";

        // Act
        var response = await _client.GetAsync($"/api/weather/{Uri.EscapeDataString(cityName)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var weather = JsonSerializer.Deserialize<WeatherResponse>(content, _jsonOptions);
        
        weather.Should().NotBeNull();
        weather!.Name.Should().NotBeNullOrEmpty();
        weather.Main.Should().NotBeNull();
        weather.Weather.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetWeatherByCity_WithInvalidCity_ShouldReturnNotFound()
    {
        // Arrange
        var cityName = "NonExistentCity12345";

        // Act
        var response = await _client.GetAsync($"/api/weather/{Uri.EscapeDataString(cityName)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWeatherByCoordinates_WithValidCoordinates_ShouldReturnOk()
    {
        // Arrange
        var lat = -23.5505;
        var lon = -46.6333;

        // Act
        var response = await _client.GetAsync($"/api/weather/coordinates?lat={lat}&lon={lon}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var weather = JsonSerializer.Deserialize<WeatherResponse>(content, _jsonOptions);
        
        weather.Should().NotBeNull();
        weather!.Coord.Lat.Should().BeApproximately(lat, 0.1);
        weather.Coord.Lon.Should().BeApproximately(lon, 0.1);
    }

    [Theory]
    [InlineData(91, 0)] // Invalid latitude > 90
    [InlineData(-91, 0)] // Invalid latitude < -90
    [InlineData(0, 181)] // Invalid longitude > 180
    [InlineData(0, -181)] // Invalid longitude < -180
    public async Task GetWeatherByCoordinates_WithInvalidCoordinates_ShouldReturnBadRequest(double lat, double lon)
    {
        // Act
        var response = await _client.GetAsync($"/api/weather/coordinates?lat={lat}&lon={lon}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetFavorites_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/weather/favorites");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFavorites_WithAuthentication_ShouldReturnOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/weather/favorites");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var favorites = JsonSerializer.Deserialize<List<WeatherFavoriteResponseDto>>(content, _jsonOptions);
        
        favorites.Should().NotBeNull();
        favorites.Should().BeEmpty(); // Initially empty
    }

    [Fact]
    public async Task AddFavorite_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var favoriteRequest = new WeatherFavoriteRequestDto
        {
            CityName = "Rio de Janeiro",
            CountryCode = "BR",
            Latitude = -22.9068,
            Longitude = -43.1729
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/weather/favorites", favoriteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var favorite = JsonSerializer.Deserialize<WeatherFavoriteResponseDto>(content, _jsonOptions);
        
        favorite.Should().NotBeNull();
        favorite!.CityName.Should().Be(favoriteRequest.CityName);
        favorite.CountryCode.Should().Be(favoriteRequest.CountryCode);
        favorite.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddFavorite_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var favoriteRequest = new WeatherFavoriteRequestDto
        {
            CityName = "Rio de Janeiro",
            CountryCode = "BR",
            Latitude = -22.9068,
            Longitude = -43.1729
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/weather/favorites", favoriteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddFavorite_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var favoriteRequest = new WeatherFavoriteRequestDto
        {
            CityName = "", // Invalid empty city name
            CountryCode = "BR",
            Latitude = -22.9068,
            Longitude = -43.1729
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/weather/favorites", favoriteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteFavorite_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First add a favorite
        var favoriteRequest = new WeatherFavoriteRequestDto
        {
            CityName = "Brasília",
            CountryCode = "BR",
            Latitude = -15.7801,
            Longitude = -47.9292
        };

        var addResponse = await _client.PostAsJsonAsync("/api/weather/favorites", favoriteRequest);
        var addContent = await addResponse.Content.ReadAsStringAsync();
        var addedFavorite = JsonSerializer.Deserialize<WeatherFavoriteResponseDto>(addContent, _jsonOptions);

        // Act
        var response = await _client.DeleteAsync($"/api/weather/favorites/{addedFavorite!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteFavorite_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/weather/favorites/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteFavorite_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var favoriteId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/weather/favorites/{favoriteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WeatherFavoritesWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var favoriteRequest = new WeatherFavoriteRequestDto
        {
            CityName = "Salvador",
            CountryCode = "BR",
            Latitude = -12.9714,
            Longitude = -38.5014
        };

        // Act & Assert - Add favorite
        var addResponse = await _client.PostAsJsonAsync("/api/weather/favorites", favoriteRequest);
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act & Assert - Get favorites (should contain the added one)
        var getFavoritesResponse = await _client.GetAsync("/api/weather/favorites");
        getFavoritesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var favoritesContent = await getFavoritesResponse.Content.ReadAsStringAsync();
        var favorites = JsonSerializer.Deserialize<List<WeatherFavoriteResponseDto>>(favoritesContent, _jsonOptions);
        
        favorites.Should().HaveCount(1);
        favorites!.First().CityName.Should().Be(favoriteRequest.CityName);

        // Act & Assert - Delete favorite
        var favoriteId = favorites.First().Id;
        var deleteResponse = await _client.DeleteAsync($"/api/weather/favorites/{favoriteId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act & Assert - Get favorites (should be empty again)
        var getFavoritesAfterDeleteResponse = await _client.GetAsync("/api/weather/favorites");
        getFavoritesAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var favoritesAfterDeleteContent = await getFavoritesAfterDeleteResponse.Content.ReadAsStringAsync();
        var favoritesAfterDelete = JsonSerializer.Deserialize<List<WeatherFavoriteResponseDto>>(favoritesAfterDeleteContent, _jsonOptions);
        
        favoritesAfterDelete.Should().BeEmpty();
    }
}