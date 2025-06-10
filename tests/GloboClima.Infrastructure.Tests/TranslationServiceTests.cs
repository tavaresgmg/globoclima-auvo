using FluentAssertions;
using GloboClima.Infrastructure.Services;
using Xunit;

namespace GloboClima.Infrastructure.Tests;

public class TranslationServiceTests
{
    [Theory]
    [InlineData("clear sky", "céu limpo")]
    [InlineData("few clouds", "poucas nuvens")]
    [InlineData("sunny", "ensolarado")]
    [InlineData("partly cloudy", "parcialmente nublado")]
    [InlineData("rain", "chuva")]
    [InlineData("thunderstorm", "tempestade")]
    [InlineData("unknown condition", "unknown condition")] // Should return original if not found
    public void TranslateWeatherCondition_ShouldReturnCorrectTranslation(string input, string expected)
    {
        // Act
        var result = TranslationService.TranslateWeatherCondition(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Brazil", "Brasil")]
    [InlineData("United States", "Estados Unidos")]
    [InlineData("Germany", "Alemanha")]
    [InlineData("France", "França")]
    [InlineData("Spain", "Espanha")]
    [InlineData("Unknown Country", "Unknown Country")] // Should return original if not found
    public void TranslateCountryName_ShouldReturnCorrectTranslation(string input, string expected)
    {
        // Act
        var result = TranslationService.TranslateCountryName(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void TranslateWeatherCondition_WithNullOrEmpty_ShouldReturnOriginal()
    {
        // Act & Assert
        TranslationService.TranslateWeatherCondition(null).Should().BeNull();
        TranslationService.TranslateWeatherCondition("").Should().Be("");
        TranslationService.TranslateWeatherCondition("   ").Should().Be("   ");
    }

    [Fact]
    public void TranslateCountryName_WithNullOrEmpty_ShouldReturnOriginal()
    {
        // Act & Assert
        TranslationService.TranslateCountryName(null).Should().BeNull();
        TranslationService.TranslateCountryName("").Should().Be("");
        TranslationService.TranslateCountryName("   ").Should().Be("   ");
    }

    [Fact]
    public void TranslateWeatherCondition_ShouldBeCaseInsensitive()
    {
        // Act & Assert
        TranslationService.TranslateWeatherCondition("SUNNY").Should().Be("ensolarado");
        TranslationService.TranslateWeatherCondition("Sunny").Should().Be("ensolarado");
        TranslationService.TranslateWeatherCondition("sunny").Should().Be("ensolarado");
        TranslationService.TranslateWeatherCondition("SuNnY").Should().Be("ensolarado");
    }
}