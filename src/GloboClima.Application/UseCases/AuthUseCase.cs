using GloboClima.Application.DTOs.Auth;
using GloboClima.Application.Interfaces;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Domain.Interfaces.Services;

namespace GloboClima.Application.UseCases;

public class AuthUseCase : IAuthUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public AuthUseCase(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return null;

        if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
            return null;

        var token = await _authService.GenerateJwtTokenAsync(user.Id, user.Email, user.FirstName, user.LastName);
        
        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            return null;

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _authService.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user);
        var token = await _authService.GenerateJwtTokenAsync(createdUser.Id, createdUser.Email, createdUser.FirstName, createdUser.LastName);
        
        return new AuthResponseDto
        {
            Token = token,
            Email = createdUser.Email,
            FirstName = createdUser.FirstName,
            LastName = createdUser.LastName,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }
}