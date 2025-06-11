using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GloboClima.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GloboClima.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly string _jwtSecret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
            _configuration["Jwt:Secret"] ?? 
            throw new InvalidOperationException("JWT Secret not configured. Set JWT_SECRET environment variable.");
        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
            _configuration["Jwt:Issuer"] ?? 
            "GloboClima";
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
            _configuration["Jwt:Audience"] ?? 
            "GloboClima";
        var expirationStr = Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") ?? 
            _configuration["Jwt:ExpirationMinutes"] ?? 
            "60";
        _expirationMinutes = int.Parse(expirationStr);
    }

    public Task<string> GenerateJwtTokenAsync(Guid userId, string email, string firstName, string lastName)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        var expiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName),
                new Claim(ClaimTypes.Name, $"{firstName} {lastName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            }),
            Expires = expiresAt,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(tokenHandler.WriteToken(token));
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}