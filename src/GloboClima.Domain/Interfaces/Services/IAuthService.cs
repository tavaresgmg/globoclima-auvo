namespace GloboClima.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<string> GenerateJwtTokenAsync(Guid userId, string email);
    bool ValidateToken(string token);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}