namespace GloboClima.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<string> GenerateJwtTokenAsync(Guid userId, string email, string firstName, string lastName);
    bool ValidateToken(string token);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}