using FluentAssertions;
using GloboClima.Application.DTOs.Weather;
using GloboClima.Application.UseCases;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Domain.Interfaces.Services;
using GloboClima.Domain.Models.Weather;
using Moq;
using Xunit;

namespace GloboClima.Application.Tests;

public class WeatherUseCaseTests
{
    private readonly Mock<IWeatherService> _weatherServiceMock;
    private readonly Mock<IWeatherFavoriteRepository> _weatherFavoriteRepositoryMock;
    private readonly WeatherUseCase _weatherUseCase;

    public WeatherUseCaseTests()
    {
        _weatherServiceMock = new Mock<IWeatherService>();
        _weatherFavoriteRepositoryMock = new Mock<IWeatherFavoriteRepository>();
        _weatherUseCase = new WeatherUseCase(_weatherServiceMock.Object, _weatherFavoriteRepositoryMock.Object);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithValidCity_ShouldReturnWeatherResponse()
    {
        // Arrange
        var cityName = "São Paulo";
        var expectedWeather = new WeatherResponse
        {
            Name = cityName,
            Main = new Main { Temp = 25.0, Humidity = 65 },
            Weather = new List<WeatherInfo> 
            { 
                new WeatherInfo { Main = "Clear", Description = "céu limpo" } 
            },
            Sys = new Sys { Country = "Brasil" }
        };

        _weatherServiceMock
            .Setup(x => x.GetWeatherByCityAsync(cityName))
            .ReturnsAsync(expectedWeather);

        // Act
        var result = await _weatherUseCase.GetWeatherByCityAsync(cityName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedWeather);
        _weatherServiceMock.Verify(x => x.GetWeatherByCityAsync(cityName), Times.Once);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithInvalidCity_ShouldReturnNull()
    {
        // Arrange
        var cityName = "NonExistentCity";

        _weatherServiceMock
            .Setup(x => x.GetWeatherByCityAsync(cityName))
            .ReturnsAsync((WeatherResponse?)null);

        // Act
        var result = await _weatherUseCase.GetWeatherByCityAsync(cityName);

        // Assert
        result.Should().BeNull();
        _weatherServiceMock.Verify(x => x.GetWeatherByCityAsync(cityName), Times.Once);
    }

    [Fact]
    public async Task AddFavoriteAsync_WithValidRequest_ShouldReturnFavoriteResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new WeatherFavoriteRequestDto
        {
            CityName = "Rio de Janeiro",
            CountryCode = "BR",
            Latitude = -22.9068,
            Longitude = -43.1729
        };

        var expectedFavorite = new WeatherFavorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CityName = request.CityName,
            CountryCode = request.CountryCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AddedAt = DateTime.UtcNow
        };

        _weatherFavoriteRepositoryMock
            .Setup(x => x.ExistsAsync(userId, request.CityName, request.CountryCode))
            .ReturnsAsync(false);

        _weatherFavoriteRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<WeatherFavorite>()))
            .ReturnsAsync(expectedFavorite);

        // Act
        var result = await _weatherUseCase.AddFavoriteAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.CityName.Should().Be(request.CityName);
        result.CountryCode.Should().Be(request.CountryCode);
        result.Latitude.Should().Be(request.Latitude);
        result.Longitude.Should().Be(request.Longitude);
        
        _weatherFavoriteRepositoryMock.Verify(x => x.CreateAsync(It.Is<WeatherFavorite>(f => 
            f.UserId == userId && 
            f.CityName == request.CityName &&
            f.CountryCode == request.CountryCode)), Times.Once);
    }

    [Fact]
    public async Task AddFavoriteAsync_WithExistingFavorite_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new WeatherFavoriteRequestDto
        {
            CityName = "Rio de Janeiro",
            CountryCode = "BR",
            Latitude = -22.9068,
            Longitude = -43.1729
        };

        _weatherFavoriteRepositoryMock
            .Setup(x => x.ExistsAsync(userId, request.CityName, request.CountryCode))
            .ReturnsAsync(true);

        // Act
        var result = await _weatherUseCase.AddFavoriteAsync(userId, request);

        // Assert
        result.Should().BeNull();
        _weatherFavoriteRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<WeatherFavorite>()), Times.Never);
    }

    [Fact]
    public async Task GetFavoritesAsync_WithValidUserId_ShouldReturnFavoritesList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var favorites = new List<WeatherFavorite>
        {
            new WeatherFavorite 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId, 
                CityName = "São Paulo", 
                CountryCode = "BR",
                Latitude = -23.5505,
                Longitude = -46.6333,
                AddedAt = DateTime.UtcNow
            },
            new WeatherFavorite 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId, 
                CityName = "Rio de Janeiro", 
                CountryCode = "BR",
                Latitude = -22.9068,
                Longitude = -43.1729,
                AddedAt = DateTime.UtcNow
            }
        };

        var weatherResponse = new WeatherResponse
        {
            Name = "São Paulo",
            Main = new Main 
            { 
                Temp = 25.0, 
                FeelsLike = 28.0, 
                Humidity = 65 
            },
            Weather = new List<WeatherInfo> 
            { 
                new WeatherInfo 
                { 
                    Main = "Clear", 
                    Description = "céu limpo", 
                    Icon = "01d" 
                } 
            },
            Wind = new Wind { Speed = 3.5 }
        };

        _weatherFavoriteRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(favorites);

        _weatherServiceMock
            .Setup(x => x.GetWeatherByCityAsync(It.IsAny<string>()))
            .ReturnsAsync(weatherResponse);

        // Act
        var result = await _weatherUseCase.GetFavoritesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().CityName.Should().Be("São Paulo");
        result.Last().CityName.Should().Be("Rio de Janeiro");
        
        _weatherFavoriteRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var favoriteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var favorite = new WeatherFavorite
        {
            Id = favoriteId,
            UserId = userId,
            CityName = "São Paulo",
            CountryCode = "BR"
        };

        _weatherFavoriteRepositoryMock
            .Setup(x => x.GetByIdAsync(favoriteId))
            .ReturnsAsync(favorite);

        // Act
        var result = await _weatherUseCase.RemoveFavoriteAsync(userId, favoriteId);

        // Assert
        result.Should().BeTrue();
        _weatherFavoriteRepositoryMock.Verify(x => x.DeleteAsync(favoriteId), Times.Once);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var favoriteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _weatherFavoriteRepositoryMock
            .Setup(x => x.GetByIdAsync(favoriteId))
            .ReturnsAsync((WeatherFavorite?)null);

        // Act
        var result = await _weatherUseCase.RemoveFavoriteAsync(userId, favoriteId);

        // Assert
        result.Should().BeFalse();
        _weatherFavoriteRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        var favoriteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var favorite = new WeatherFavorite
        {
            Id = favoriteId,
            UserId = differentUserId,
            CityName = "São Paulo",
            CountryCode = "BR"
        };

        _weatherFavoriteRepositoryMock
            .Setup(x => x.GetByIdAsync(favoriteId))
            .ReturnsAsync(favorite);

        // Act
        var result = await _weatherUseCase.RemoveFavoriteAsync(userId, favoriteId);

        // Assert
        result.Should().BeFalse();
        _weatherFavoriteRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}