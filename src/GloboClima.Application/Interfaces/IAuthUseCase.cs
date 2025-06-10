using GloboClima.Application.DTOs.Auth;

namespace GloboClima.Application.Interfaces;

public interface IAuthUseCase
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request);
}