using FluentAssertions;
using GloboClima.Domain.Entities;
using GloboClima.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GloboClima.Infrastructure.Tests;

public class AuthServiceTests
{
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Secret", "THIS_IS_A_TEST_SECRET_KEY_WITH_AT_LEAST_32_CHARACTERS"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpirationInMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _authService = new AuthService(_configuration);
    }

    [Fact]
    public void HashPassword_Should_Return_Different_Hash_For_Same_Password()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _authService.HashPassword(password);
        var hash2 = _authService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void VerifyPassword_Should_Return_True_For_Valid_Password()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Invalid_Password()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateJwtToken_Should_Return_Valid_Token()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");
        var parts = token.Split('.');
        parts.Should().HaveCount(3); // JWT has 3 parts: header.payload.signature
    }

    [Fact]
    public void GenerateJwtToken_Should_Include_User_Claims()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        // Decode token to verify claims
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        jsonToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == user.Id.ToString());
        jsonToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
        jsonToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == user.Name);
        jsonToken.Claims.Should().Contain(c => c.Type == "jti"); // JWT ID
        jsonToken.Claims.Should().Contain(c => c.Type == "iat"); // Issued At
    }

    [Fact]
    public void GenerateJwtToken_Should_Set_Correct_Expiration()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        var now = DateTime.UtcNow;
        jsonToken.ValidTo.Should().BeCloseTo(now.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateJwtToken_Should_Set_Correct_Issuer_And_Audience()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        jsonToken.Issuer.Should().Be("TestIssuer");
        jsonToken.Audiences.Should().Contain("TestAudience");
    }
}