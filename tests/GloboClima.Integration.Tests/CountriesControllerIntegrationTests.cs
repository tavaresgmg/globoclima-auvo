using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GloboClima.Application.DTOs.Auth;
using GloboClima.Application.DTOs.Country;
using Xunit;

namespace GloboClima.Integration.Tests;

public class CountriesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CountriesControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchCountries_WithValidName_ShouldReturnCountries()
    {
        // Act
        var response = await _client.GetAsync("/api/countries/search?name=Brazil");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
        
        var countries = JsonSerializer.Deserialize<object[]>(content);
        countries.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCountryByCode_WithValidCode_ShouldReturnCountry()
    {
        // Act
        var response = await _client.GetAsync("/api/countries/BR");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetFavorites_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/countries/favorites");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFavorites_WithAuth_ShouldReturnFavorites()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/countries/favorites");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var favorites = JsonSerializer.Deserialize<CountryFavoriteResponseDto[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        favorites.Should().NotBeNull();
    }

    [Fact]
    public async Task AddFavorite_WithAuth_ShouldCreateFavorite()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new CountryFavoriteRequestDto
        {
            CountryCode = "JP",
            CountryName = "Japan",
            Region = "Asia"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/countries/favorites", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var favorite = JsonSerializer.Deserialize<CountryFavoriteResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        favorite.Should().NotBeNull();
        favorite.CountryCode.Should().Be("JP");
        favorite.CountryName.Should().Be("Japan");
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var registerRequest = new RegisterRequestDto
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            Password = "Test@123",
            ConfirmPassword = "Test@123",
            FirstName = "Test",
            LastName = "User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return authResponse.Token;
    }
}