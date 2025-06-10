using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using GloboClima.Application.Interfaces;
using GloboClima.Application.UseCases;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Domain.Interfaces.Services;
using GloboClima.Infrastructure.Repositories;
using GloboClima.Infrastructure.Services;
using GloboClima.Application.DTOs.Auth;
using GloboClima.Application.DTOs.Weather;
using GloboClima.Application.DTOs.Country;
using System.IdentityModel.Tokens.Jwt;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GloboClima.Lambda;

public class Function
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Function> _logger;

    public Function()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<Function>>();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddConsole());
        services.AddHttpClient();
        
        // Configuration for Lambda
        var configuration = new Dictionary<string, string>
        {
            {"Jwt:Secret", "AUVO-SUPER-SECRET-KEY-FOR-JWT-TOKENS-2024-GLOBOCLIMA-TEST"},
            {"Jwt:Issuer", "GloboClima"},
            {"Jwt:Audience", "GloboClima"},
            {"Jwt:ExpirationMinutes", "60"},
            {"OpenWeatherMap:ApiKey", "50ad4ee7dfb040bba30170921251006"},
            {"WeatherApi:BaseUrl", "http://api.weatherapi.com/v1"},
            {"CountriesApi:BaseUrl", "https://restcountries.com/v3.1"}
        };
        
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(configuration);
        var config = configBuilder.Build();
        services.AddSingleton<IConfiguration>(config);
        
        // Services
        services.AddScoped<IWeatherService, WeatherService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IAuthService, AuthService>();
        
        // DynamoDB Context
        services.AddScoped<GloboClima.Infrastructure.Repositories.DynamoDbContext>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            return new GloboClima.Infrastructure.Repositories.DynamoDbContext(configuration);
        });
        
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWeatherFavoriteRepository, WeatherFavoriteRepository>();
        services.AddScoped<ICountryFavoriteRepository, CountryFavoriteRepository>();
        
        // Use Cases
        services.AddScoped<IAuthUseCase, AuthUseCase>();
        services.AddScoped<IWeatherUseCase, WeatherUseCase>();
        services.AddScoped<ICountryUseCase, CountryUseCase>();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        _logger.LogInformation($"Processing request: {request.HttpMethod} {request.Path}");

        try
        {
            var response = new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "Content-Type,Authorization" },
                    { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS" },
                    { "Content-Type", "application/json" }
                }
            };

            // Handle CORS preflight
            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Body = "";
                return response;
            }

            // Route handling
            var result = await RouteRequest(request);
            response.StatusCode = result.StatusCode;
            response.Body = result.Body;
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Internal server error" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }

    private async Task<(int StatusCode, string Body)> RouteRequest(APIGatewayProxyRequest request)
    {
        var path = request.Path?.ToLower() ?? "";
        var method = request.HttpMethod?.ToUpper() ?? "";

        // Health check
        if (path == "/health")
        {
            return (200, JsonSerializer.Serialize(new { status = "healthy", service = "GloboClima.Lambda" }));
        }

        // Swagger
        if (path.StartsWith("/swagger"))
        {
            return (200, GetSwaggerJson());
        }

        // Auth endpoints
        if (path.StartsWith("/api/auth"))
        {
            return await HandleAuthEndpoints(request, path, method);
        }

        // Weather endpoints
        if (path.StartsWith("/api/weather"))
        {
            return await HandleWeatherEndpoints(request, path, method);
        }

        // Country endpoints
        if (path.StartsWith("/api/countries"))
        {
            return await HandleCountryEndpoints(request, path, method);
        }

        return (404, JsonSerializer.Serialize(new { error = "Endpoint not found" }));
    }

    private async Task<(int StatusCode, string Body)> HandleAuthEndpoints(APIGatewayProxyRequest request, string path, string method)
    {
        var authUseCase = _serviceProvider.GetRequiredService<IAuthUseCase>();

        if (path == "/api/auth/register" && method == "POST")
        {
            var registerRequest = JsonSerializer.Deserialize<RegisterRequestDto>(request.Body);
            var result = await authUseCase.RegisterAsync(registerRequest);
            return result != null ? (201, JsonSerializer.Serialize(result)) : (400, JsonSerializer.Serialize(new { error = "Registration failed" }));
        }

        if (path == "/api/auth/login" && method == "POST")
        {
            var loginRequest = JsonSerializer.Deserialize<LoginRequestDto>(request.Body);
            var result = await authUseCase.LoginAsync(loginRequest);
            return result != null ? (200, JsonSerializer.Serialize(result)) : (401, JsonSerializer.Serialize(new { error = "Invalid credentials" }));
        }

        return (404, JsonSerializer.Serialize(new { error = "Auth endpoint not found" }));
    }

    private async Task<(int StatusCode, string Body)> HandleWeatherEndpoints(APIGatewayProxyRequest request, string path, string method)
    {
        var weatherUseCase = _serviceProvider.GetRequiredService<IWeatherUseCase>();

        // Public weather endpoint
        if (path.StartsWith("/api/weather/city/") && method == "GET")
        {
            var city = path.Split('/').LastOrDefault();
            if (string.IsNullOrEmpty(city))
                return (400, JsonSerializer.Serialize(new { error = "City name is required" }));

            var result = await weatherUseCase.GetWeatherByCityAsync(city);
            return result != null ? (200, JsonSerializer.Serialize(result)) : (404, JsonSerializer.Serialize(new { error = "Weather data not found" }));
        }

        // Protected favorite endpoints
        var userId = GetUserIdFromToken(request);
        if (userId == null)
            return (401, JsonSerializer.Serialize(new { error = "Unauthorized" }));

        if (path == "/api/weather/favorites" && method == "GET")
        {
            var favorites = await weatherUseCase.GetFavoritesAsync(userId.Value);
            return (200, JsonSerializer.Serialize(favorites));
        }

        if (path == "/api/weather/favorites" && method == "POST")
        {
            var favoriteRequest = JsonSerializer.Deserialize<WeatherFavoriteRequestDto>(request.Body);
            var result = await weatherUseCase.AddFavoriteAsync(userId.Value, favoriteRequest!);
            return result != null ? (201, JsonSerializer.Serialize(result)) : (400, JsonSerializer.Serialize(new { error = "Failed to add favorite" }));
        }

        if (path.StartsWith("/api/weather/favorites/") && method == "DELETE")
        {
            var favoriteId = Guid.Parse(path.Split('/').LastOrDefault() ?? "");
            var success = await weatherUseCase.RemoveFavoriteAsync(favoriteId, userId.Value);
            return success ? (204, "") : (404, JsonSerializer.Serialize(new { error = "Favorite not found" }));
        }

        return (404, JsonSerializer.Serialize(new { error = "Weather endpoint not found" }));
    }

    private async Task<(int StatusCode, string Body)> HandleCountryEndpoints(APIGatewayProxyRequest request, string path, string method)
    {
        var countryUseCase = _serviceProvider.GetRequiredService<ICountryUseCase>();

        // Public country endpoint
        if (path.StartsWith("/api/countries/") && method == "GET" && !path.Contains("favorites"))
        {
            var countryName = path.Split('/').LastOrDefault();
            if (string.IsNullOrEmpty(countryName))
                return (400, JsonSerializer.Serialize(new { error = "Country name is required" }));

            var result = await countryUseCase.GetCountryByNameAsync(countryName);
            return result != null ? (200, JsonSerializer.Serialize(result)) : (404, JsonSerializer.Serialize(new { error = "Country not found" }));
        }

        // Protected favorite endpoints
        var userId = GetUserIdFromToken(request);
        if (userId == null)
            return (401, JsonSerializer.Serialize(new { error = "Unauthorized" }));

        if (path == "/api/countries/favorites" && method == "GET")
        {
            var favorites = await countryUseCase.GetFavoritesAsync(userId.Value);
            return (200, JsonSerializer.Serialize(favorites));
        }

        if (path == "/api/countries/favorites" && method == "POST")
        {
            var favoriteRequest = JsonSerializer.Deserialize<CountryFavoriteRequestDto>(request.Body);
            var result = await countryUseCase.AddFavoriteAsync(userId.Value, favoriteRequest!);
            return result != null ? (201, JsonSerializer.Serialize(result)) : (400, JsonSerializer.Serialize(new { error = "Failed to add favorite" }));
        }

        if (path.StartsWith("/api/countries/favorites/") && method == "DELETE")
        {
            var favoriteId = Guid.Parse(path.Split('/').LastOrDefault() ?? "");
            var success = await countryUseCase.RemoveFavoriteAsync(favoriteId, userId.Value);
            return success ? (204, "") : (404, JsonSerializer.Serialize(new { error = "Favorite not found" }));
        }

        return (404, JsonSerializer.Serialize(new { error = "Country endpoint not found" }));
    }

    private Guid? GetUserIdFromToken(APIGatewayProxyRequest request)
    {
        if (!request.Headers.TryGetValue("Authorization", out var authHeader))
            return null;

        if (!authHeader.StartsWith("Bearer "))
            return null;

        try
        {
            var token = authHeader.Substring(7);
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }

    private string GetSwaggerJson()
    {
        var swagger = new
        {
            openapi = "3.0.1",
            info = new
            {
                title = "GloboClima API - AUVO Technical Test",
                version = "1.0.0",
                description = "REST API for weather and country information with favorites management"
            },
            servers = new[] { new { url = "https://api.gateway.url/prod", description = "Production" } },
            paths = new
            {
                // Auth endpoints
                _api_auth_register = new
                {
                    post = new
                    {
                        tags = new[] { "Authentication" },
                        summary = "Register new user",
                        requestBody = new
                        {
                            content = new
                            {
                                _application_json = new
                                {
                                    schema = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            email = new { type = "string", format = "email" },
                                            password = new { type = "string", minLength = 6 }
                                        }
                                    }
                                }
                            }
                        },
                        responses = new
                        {
                            _201 = new { description = "User registered successfully" },
                            _400 = new { description = "Invalid input" }
                        }
                    }
                },
                _api_auth_login = new
                {
                    post = new
                    {
                        tags = new[] { "Authentication" },
                        summary = "Login user",
                        requestBody = new
                        {
                            content = new
                            {
                                _application_json = new
                                {
                                    schema = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            email = new { type = "string", format = "email" },
                                            password = new { type = "string" }
                                        }
                                    }
                                }
                            }
                        },
                        responses = new
                        {
                            _200 = new { description = "Login successful" },
                            _401 = new { description = "Invalid credentials" }
                        }
                    }
                }
            }
        };

        return JsonSerializer.Serialize(swagger, new JsonSerializerOptions { WriteIndented = true });
    }
}