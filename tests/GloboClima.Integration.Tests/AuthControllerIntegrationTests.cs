using FluentAssertions;
using GloboClima.Application.DTOs.Auth;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GloboClima.Integration.Tests;

public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Email = "test@globoclima.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(content, _jsonOptions);
        
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Email.Should().Be(registerRequest.Email);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Email = "invalid-email",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Email = "test2@globoclima.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange - First register a user
        var registerRequest = new RegisterRequestDto
        {
            Email = "login-test@globoclima.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = "login-test@globoclima.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(content, _jsonOptions);
        
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Email.Should().Be(loginRequest.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "nonexistent@globoclima.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyCredentials_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "",
            Password = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var email = "duplicate@globoclima.com";
        var registerRequest1 = new RegisterRequestDto
        {
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        
        var registerRequest2 = new RegisterRequestDto
        {
            Email = email,
            Password = "DifferentPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest1);
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("", "Password123!", "Password123!")] // Empty email
    [InlineData("test@example.com", "", "")] // Empty password
    [InlineData("test@example.com", "weak", "weak")] // Weak password
    [InlineData("invalid-email", "Password123!", "Password123!")] // Invalid email format
    public async Task Register_WithInvalidData_ShouldReturnBadRequest(string email, string password, string confirmPassword)
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}