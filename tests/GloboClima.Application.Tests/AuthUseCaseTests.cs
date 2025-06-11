using GloboClima.Application.DTOs.Auth;
using GloboClima.Application.UseCases;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Domain.Interfaces.Services;
using Moq;
using Xunit;

namespace GloboClima.Application.Tests;

public class AuthUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthUseCase _authUseCase;

    public AuthUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _authServiceMock = new Mock<IAuthService>();
        _authUseCase = new AuthUseCase(_userRepositoryMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = loginRequest.Email,
            PasswordHash = "hashedPassword",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);

        _authServiceMock.Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(true);

        _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user.Id, user.Email, user.FirstName, user.LastName))
            .ReturnsAsync("jwt-token");

        // Act
        var result = await _authUseCase.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.FirstName, result.FirstName);
        Assert.Equal("jwt-token", result.Token);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = loginRequest.Email,
            PasswordHash = "hashedPassword",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);

        _authServiceMock.Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authUseCase.LoginAsync(loginRequest);

        // Assert
        Assert.Null(result);
    }
}