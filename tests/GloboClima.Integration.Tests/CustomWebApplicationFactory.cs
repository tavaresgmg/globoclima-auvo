using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GloboClima.Integration.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing first
        builder.UseEnvironment("Testing");
        
        // Configure test-specific settings
        builder.UseSetting("JwtSettings:Secret", "test-super-secret-key-for-integration-tests-only-32-chars");
        builder.UseSetting("JwtSettings:Issuer", "GloboClima");
        builder.UseSetting("JwtSettings:Audience", "GloboClima");
        builder.UseSetting("JwtSettings:ExpirationHours", "1");
        builder.UseSetting("WeatherApiKey", "test-api-key");
        builder.UseSetting("DynamoDB:UseLocalDb", "true");

        builder.ConfigureServices(services =>
        {
            // Remove existing repository services
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IWeatherFavoriteRepository>();
            services.RemoveAll<ICountryFavoriteRepository>();

            // Replace with in-memory implementations for testing
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddSingleton<IWeatherFavoriteRepository, InMemoryWeatherFavoriteRepository>();
            services.AddSingleton<ICountryFavoriteRepository, InMemoryCountryFavoriteRepository>();
        });
    }
}