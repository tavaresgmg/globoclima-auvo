using FluentAssertions;
using GloboClima.Application.DTOs.Country;
using GloboClima.Application.UseCases;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Domain.Interfaces.Services;
using GloboClima.Domain.Models.Country;
using Moq;
using Xunit;

namespace GloboClima.Application.Tests;

public class CountryUseCaseTests
{
    private readonly Mock<ICountryService> _countryServiceMock;
    private readonly Mock<ICountryFavoriteRepository> _countryFavoriteRepositoryMock;
    private readonly CountryUseCase _countryUseCase;

    public CountryUseCaseTests()
    {
        _countryServiceMock = new Mock<ICountryService>();
        _countryFavoriteRepositoryMock = new Mock<ICountryFavoriteRepository>();
        _countryUseCase = new CountryUseCase(_countryServiceMock.Object, _countryFavoriteRepositoryMock.Object);
    }

    [Fact]
    public async Task AddFavoriteAsync_WithValidRequest_ShouldReturnFavoriteResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CountryFavoriteRequestDto
        {
            CountryCode = "BR",
            CountryName = "Brasil",
            Region = "Americas"
        };

        var expectedFavorite = new CountryFavorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CountryCode = request.CountryCode,
            CountryName = request.CountryName,
            Region = request.Region,
            AddedAt = DateTime.UtcNow
        };

        _countryFavoriteRepositoryMock
            .Setup(x => x.ExistsAsync(userId, request.CountryCode))
            .ReturnsAsync(false);

        _countryFavoriteRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<CountryFavorite>()))
            .ReturnsAsync(expectedFavorite);

        // Act
        var result = await _countryUseCase.AddFavoriteAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.CountryCode.Should().Be(request.CountryCode);
        result.CountryName.Should().Be(request.CountryName);
        result.Region.Should().Be(request.Region);
        
        _countryFavoriteRepositoryMock.Verify(x => x.CreateAsync(It.Is<CountryFavorite>(f => 
            f.UserId == userId && 
            f.CountryCode == request.CountryCode &&
            f.CountryName == request.CountryName)), Times.Once);
    }

    [Fact]
    public async Task AddFavoriteAsync_WithExistingFavorite_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CountryFavoriteRequestDto
        {
            CountryCode = "BR",
            CountryName = "Brasil",
            Region = "Americas"
        };

        _countryFavoriteRepositoryMock
            .Setup(x => x.ExistsAsync(userId, request.CountryCode))
            .ReturnsAsync(true);

        // Act
        var result = await _countryUseCase.AddFavoriteAsync(userId, request);

        // Assert
        result.Should().BeNull();
        _countryFavoriteRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<CountryFavorite>()), Times.Never);
    }

    [Fact]
    public async Task GetFavoritesAsync_WithValidUserId_ShouldReturnFavoritesList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var favorites = new List<CountryFavorite>
        {
            new CountryFavorite 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId, 
                CountryCode = "BR", 
                CountryName = "Brasil",
                Region = "Americas",
                AddedAt = DateTime.UtcNow
            },
            new CountryFavorite 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId, 
                CountryCode = "US", 
                CountryName = "Estados Unidos",
                Region = "Americas",
                AddedAt = DateTime.UtcNow
            }
        };

        var countryInfo = new CountryResponse
        {
            Name = new Name { Common = "Brasil" },
            Capital = new List<string> { "Brasília" },
            Region = "Americas",
            Subregion = "South America",
            Population = 215313498,
            Area = 8515767.0,
            Flags = new Flags { Svg = "https://flagcdn.com/br.svg" },
            Languages = new Dictionary<string, string> { { "por", "Portuguese" } },
            Currencies = new Dictionary<string, Currency> 
            { 
                { "BRL", new Currency { Name = "Brazilian real" } } 
            }
        };

        _countryFavoriteRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(favorites);

        _countryServiceMock
            .Setup(x => x.GetCountryByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(countryInfo);

        // Act
        var result = await _countryUseCase.GetFavoritesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().CountryName.Should().Be("Brasil");
        result.Last().CountryName.Should().Be("Estados Unidos");
        
        _countryFavoriteRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var favoriteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var favorite = new CountryFavorite
        {
            Id = favoriteId,
            UserId = userId,
            CountryCode = "BR",
            CountryName = "Brasil"
        };

        _countryFavoriteRepositoryMock
            .Setup(x => x.GetByIdAsync(favoriteId))
            .ReturnsAsync(favorite);

        // Act
        var result = await _countryUseCase.RemoveFavoriteAsync(userId, favoriteId);

        // Assert
        result.Should().BeTrue();
        _countryFavoriteRepositoryMock.Verify(x => x.DeleteAsync(favoriteId), Times.Once);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var favoriteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _countryFavoriteRepositoryMock
            .Setup(x => x.GetByIdAsync(favoriteId))
            .ReturnsAsync((CountryFavorite?)null);

        // Act
        var result = await _countryUseCase.RemoveFavoriteAsync(userId, favoriteId);

        // Assert
        result.Should().BeFalse();
        _countryFavoriteRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        var favoriteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var favorite = new CountryFavorite
        {
            Id = favoriteId,
            UserId = differentUserId,
            CountryCode = "BR",
            CountryName = "Brasil"
        };

        _countryFavoriteRepositoryMock
            .Setup(x => x.GetByIdAsync(favoriteId))
            .ReturnsAsync(favorite);

        // Act
        var result = await _countryUseCase.RemoveFavoriteAsync(userId, favoriteId);

        // Assert
        result.Should().BeFalse();
        _countryFavoriteRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}