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
using System.Security.Claims;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GloboClima.Lambda;

public class Function
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Function> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public Function()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<Function>>();
        
        // Configure JSON serialization options for case-insensitive deserialization
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddConsole());
        services.AddHttpClient();
        
        // Configuration for Lambda
        var configuration = new Dictionary<string, string>
        {
            {"Jwt:Secret", Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET not configured")},
            {"Jwt:Issuer", Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "GloboClima"},
            {"Jwt:Audience", Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "GloboClima"},
            {"Jwt:ExpirationMinutes", Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") ?? "60"},
            {"OpenWeatherMap:ApiKey", Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY") ?? ""},
            {"WeatherApi:ApiKey", Environment.GetEnvironmentVariable("WEATHERAPI_KEY") ?? ""},
            {"WeatherApi:BaseUrl", "http://api.weatherapi.com/v1"},
            {"CountriesApi:BaseUrl", "https://restcountries.com/v3.1"},
            {"WeatherService:UseOpenWeatherMap", Environment.GetEnvironmentVariable("USE_OPENWEATHERMAP") ?? "false"}
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
        
        // Debug: log all request properties
        _logger.LogInformation($"Resource: {request.Resource}");
        _logger.LogInformation($"PathParameters: {JsonSerializer.Serialize(request.PathParameters, _jsonOptions)}");
        _logger.LogInformation($"RequestContext.Path: {request.RequestContext?.Path}");
        _logger.LogInformation($"RequestContext.ResourcePath: {request.RequestContext?.ResourcePath}");

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
            
            // Update content type if specified
            if (!string.IsNullOrEmpty(result.ContentType))
            {
                response.Headers["Content-Type"] = result.ContentType;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Internal server error" }, _jsonOptions),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }

    private async Task<(int StatusCode, string Body, string ContentType)> RouteRequest(APIGatewayProxyRequest request)
    {
        // Try different path sources for Lambda Function URL compatibility
        var path = request.Path?.ToLower() ?? "";
        if (string.IsNullOrEmpty(path))
        {
            path = request.Resource?.ToLower() ?? "";
        }
        if (string.IsNullOrEmpty(path))
        {
            path = request.RequestContext?.Path?.ToLower() ?? "";
        }
        if (string.IsNullOrEmpty(path))
        {
            path = request.RequestContext?.ResourcePath?.ToLower() ?? "";
        }
        
        var method = request.HttpMethod?.ToUpper() ?? "";
        
        // Log for debugging
        _logger.LogInformation($"RouteRequest: path={path}, method={method}");

        // Health check
        if (path == "/health")
        {
            return (200, JsonSerializer.Serialize(new { status = "healthy", service = "GloboClima.Lambda" }, _jsonOptions), "application/json");
        }

        // Swagger
        if (path == "/swagger" || path == "/swagger/")
        {
            return (200, GetSwaggerJson(), "application/json");
        }
        
        if (path == "/swagger/index.html")
        {
            return (200, GetSwaggerHtml(), "text/html");
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

        return (404, JsonSerializer.Serialize(new { error = "Endpoint not found" }, _jsonOptions), "application/json");
    }

    private async Task<(int StatusCode, string Body, string ContentType)> HandleAuthEndpoints(APIGatewayProxyRequest request, string path, string method)
    {
        var authUseCase = _serviceProvider.GetRequiredService<IAuthUseCase>();

        if (path == "/api/auth/register" && method == "POST")
        {
            var registerRequest = JsonSerializer.Deserialize<RegisterRequestDto>(request.Body, _jsonOptions);
            var result = await authUseCase.RegisterAsync(registerRequest);
            return result != null ? (201, JsonSerializer.Serialize(result, _jsonOptions), "application/json") : (400, JsonSerializer.Serialize(new { error = "Registration failed" }, _jsonOptions), "application/json");
        }

        if (path == "/api/auth/login" && method == "POST")
        {
            var loginRequest = JsonSerializer.Deserialize<LoginRequestDto>(request.Body, _jsonOptions);
            var result = await authUseCase.LoginAsync(loginRequest);
            return result != null ? (200, JsonSerializer.Serialize(result, _jsonOptions), "application/json") : (401, JsonSerializer.Serialize(new { error = "Invalid credentials" }, _jsonOptions), "application/json");
        }

        return (404, JsonSerializer.Serialize(new { error = "Auth endpoint not found" }, _jsonOptions), "application/json");
    }

    private async Task<(int StatusCode, string Body, string ContentType)> HandleWeatherEndpoints(APIGatewayProxyRequest request, string path, string method)
    {
        var weatherUseCase = _serviceProvider.GetRequiredService<IWeatherUseCase>();

        // Public weather endpoint
        if (path.StartsWith("/api/weather/city/") && method == "GET")
        {
            var city = path.Split('/').LastOrDefault();
            if (string.IsNullOrEmpty(city))
                return (400, JsonSerializer.Serialize(new { error = "City name is required" }, _jsonOptions), "application/json");

            var result = await weatherUseCase.GetWeatherByCityAsync(city);
            return result != null ? (200, JsonSerializer.Serialize(result, _jsonOptions), "application/json") : (404, JsonSerializer.Serialize(new { error = "Weather data not found" }, _jsonOptions), "application/json");
        }

        // Protected favorite endpoints
        var userId = GetUserIdFromToken(request);
        if (userId == null)
            return (401, JsonSerializer.Serialize(new { error = "Unauthorized" }, _jsonOptions), "application/json");

        if (path == "/api/weather/favorites" && method == "GET")
        {
            var favorites = await weatherUseCase.GetFavoritesAsync(userId.Value);
            return (200, JsonSerializer.Serialize(favorites, _jsonOptions), "application/json");
        }

        if (path == "/api/weather/favorites" && method == "POST")
        {
            var favoriteRequest = JsonSerializer.Deserialize<WeatherFavoriteRequestDto>(request.Body, _jsonOptions);
            var result = await weatherUseCase.AddFavoriteAsync(userId.Value, favoriteRequest!);
            return result != null ? (201, JsonSerializer.Serialize(result, _jsonOptions), "application/json") : (400, JsonSerializer.Serialize(new { error = "Failed to add favorite" }, _jsonOptions), "application/json");
        }

        if (path.StartsWith("/api/weather/favorites/") && method == "DELETE")
        {
            var favoriteId = Guid.Parse(path.Split('/').LastOrDefault() ?? "");
            var success = await weatherUseCase.RemoveFavoriteAsync(favoriteId, userId.Value);
            return success ? (204, "", "application/json") : (404, JsonSerializer.Serialize(new { error = "Favorite not found" }, _jsonOptions), "application/json");
        }

        return (404, JsonSerializer.Serialize(new { error = "Weather endpoint not found" }, _jsonOptions), "application/json");
    }

    private async Task<(int StatusCode, string Body, string ContentType)> HandleCountryEndpoints(APIGatewayProxyRequest request, string path, string method)
    {
        var countryUseCase = _serviceProvider.GetRequiredService<ICountryUseCase>();

        // Public country endpoint
        if (path.StartsWith("/api/countries/") && method == "GET" && !path.Contains("favorites"))
        {
            var countryName = path.Split('/').LastOrDefault();
            if (string.IsNullOrEmpty(countryName))
                return (400, JsonSerializer.Serialize(new { error = "Country name is required" }, _jsonOptions), "application/json");

            var result = await countryUseCase.GetCountryByNameAsync(countryName);
            return result != null ? (200, JsonSerializer.Serialize(result, _jsonOptions), "application/json") : (404, JsonSerializer.Serialize(new { error = "Country not found" }, _jsonOptions), "application/json");
        }

        // Protected favorite endpoints
        var userId = GetUserIdFromToken(request);
        if (userId == null)
            return (401, JsonSerializer.Serialize(new { error = "Unauthorized" }, _jsonOptions), "application/json");

        if (path == "/api/countries/favorites" && method == "GET")
        {
            var favorites = await countryUseCase.GetFavoritesAsync(userId.Value);
            return (200, JsonSerializer.Serialize(favorites, _jsonOptions), "application/json");
        }

        if (path == "/api/countries/favorites" && method == "POST")
        {
            var favoriteRequest = JsonSerializer.Deserialize<CountryFavoriteRequestDto>(request.Body, _jsonOptions);
            var result = await countryUseCase.AddFavoriteAsync(userId.Value, favoriteRequest!);
            return result != null ? (201, JsonSerializer.Serialize(result, _jsonOptions), "application/json") : (400, JsonSerializer.Serialize(new { error = "Failed to add favorite" }, _jsonOptions), "application/json");
        }

        if (path.StartsWith("/api/countries/favorites/") && method == "DELETE")
        {
            var favoriteId = Guid.Parse(path.Split('/').LastOrDefault() ?? "");
            var success = await countryUseCase.RemoveFavoriteAsync(favoriteId, userId.Value);
            return success ? (204, "", "application/json") : (404, JsonSerializer.Serialize(new { error = "Favorite not found" }, _jsonOptions), "application/json");
        }

        return (404, JsonSerializer.Serialize(new { error = "Country endpoint not found" }, _jsonOptions), "application/json");
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
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            
            _logger.LogInformation($"Validating token: {token.Substring(0, 50)}...");
            
            // Validate token using AuthService
            if (!authService.ValidateToken(token))
            {
                _logger.LogWarning("Token validation failed");
                return null;
            }
                
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            
            // Look for "nameid" claim (JWT short form of NameIdentifier)
            var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;
            _logger.LogInformation($"Found user ID claim: {userIdClaim}");
            
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return null;
        }
    }

    private string GetSwaggerJson()
    {
        var apiUrl = Environment.GetEnvironmentVariable("API_URL") ?? "https://3ei1klgibg.execute-api.us-east-1.amazonaws.com/prod";
        
        var swagger = new Dictionary<string, object>
        {
            ["openapi"] = "3.0.1",
            ["info"] = new Dictionary<string, object>
            {
                ["title"] = "GloboClima API - AUVO Technical Test",
                ["version"] = "1.0.0",
                ["description"] = "REST API for weather and country information with favorites management"
            },
            ["servers"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["url"] = apiUrl,
                    ["description"] = "Production"
                }
            },
            ["paths"] = new Dictionary<string, object>
            {
                ["/api/auth/register"] = new Dictionary<string, object>
                {
                    ["post"] = new Dictionary<string, object>
                    {
                        ["tags"] = new[] { "Authentication" },
                        ["summary"] = "Register new user",
                        ["requestBody"] = new Dictionary<string, object>
                        {
                            ["required"] = true,
                            ["content"] = new Dictionary<string, object>
                            {
                                ["application/json"] = new Dictionary<string, object>
                                {
                                    ["schema"] = new Dictionary<string, object>
                                    {
                                        ["type"] = "object",
                                        ["required"] = new[] { "email", "password", "confirmPassword", "firstName", "lastName" },
                                        ["properties"] = new Dictionary<string, object>
                                        {
                                            ["email"] = new Dictionary<string, object> { ["type"] = "string", ["format"] = "email" },
                                            ["password"] = new Dictionary<string, object> { ["type"] = "string", ["minLength"] = 6 },
                                            ["confirmPassword"] = new Dictionary<string, object> { ["type"] = "string", ["minLength"] = 6 },
                                            ["firstName"] = new Dictionary<string, object> { ["type"] = "string" },
                                            ["lastName"] = new Dictionary<string, object> { ["type"] = "string" }
                                        }
                                    }
                                }
                            }
                        },
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["201"] = new Dictionary<string, object> { ["description"] = "User registered successfully" },
                            ["400"] = new Dictionary<string, object> { ["description"] = "Invalid input" }
                        }
                    }
                },
                ["/api/auth/login"] = new Dictionary<string, object>
                {
                    ["post"] = new Dictionary<string, object>
                    {
                        ["tags"] = new[] { "Authentication" },
                        ["summary"] = "Login user",
                        ["requestBody"] = new Dictionary<string, object>
                        {
                            ["required"] = true,
                            ["content"] = new Dictionary<string, object>
                            {
                                ["application/json"] = new Dictionary<string, object>
                                {
                                    ["schema"] = new Dictionary<string, object>
                                    {
                                        ["type"] = "object",
                                        ["required"] = new[] { "email", "password" },
                                        ["properties"] = new Dictionary<string, object>
                                        {
                                            ["email"] = new Dictionary<string, object> { ["type"] = "string", ["format"] = "email" },
                                            ["password"] = new Dictionary<string, object> { ["type"] = "string" }
                                        }
                                    }
                                }
                            }
                        },
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["200"] = new Dictionary<string, object> { ["description"] = "Login successful" },
                            ["401"] = new Dictionary<string, object> { ["description"] = "Invalid credentials" }
                        }
                    }
                },
                ["/api/weather/city/{city}"] = new Dictionary<string, object>
                {
                    ["get"] = new Dictionary<string, object>
                    {
                        ["tags"] = new[] { "Weather" },
                        ["summary"] = "Get weather by city",
                        ["parameters"] = new[]
                        {
                            new Dictionary<string, object>
                            {
                                ["name"] = "city",
                                ["in"] = "path",
                                ["required"] = true,
                                ["schema"] = new Dictionary<string, object> { ["type"] = "string" }
                            }
                        },
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["200"] = new Dictionary<string, object> { ["description"] = "Weather data retrieved" },
                            ["404"] = new Dictionary<string, object> { ["description"] = "City not found" }
                        }
                    }
                },
                ["/api/weather/favorites"] = new Dictionary<string, object>
                {
                    ["get"] = new Dictionary<string, object>
                    {
                        ["tags"] = new[] { "Weather" },
                        ["summary"] = "Get favorite cities",
                        ["security"] = new[] { new Dictionary<string, object[]> { ["Bearer"] = new object[0] } },
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["200"] = new Dictionary<string, object> { ["description"] = "List of favorite cities" },
                            ["401"] = new Dictionary<string, object> { ["description"] = "Unauthorized" }
                        }
                    },
                    ["post"] = new Dictionary<string, object>
                    {
                        ["tags"] = new[] { "Weather" },
                        ["summary"] = "Add favorite city",
                        ["security"] = new[] { new Dictionary<string, object[]> { ["Bearer"] = new object[0] } },
                        ["requestBody"] = new Dictionary<string, object>
                        {
                            ["required"] = true,
                            ["content"] = new Dictionary<string, object>
                            {
                                ["application/json"] = new Dictionary<string, object>
                                {
                                    ["schema"] = new Dictionary<string, object>
                                    {
                                        ["type"] = "object",
                                        ["properties"] = new Dictionary<string, object>
                                        {
                                            ["cityName"] = new Dictionary<string, object> { ["type"] = "string" }
                                        }
                                    }
                                }
                            }
                        },
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["201"] = new Dictionary<string, object> { ["description"] = "Favorite added" },
                            ["401"] = new Dictionary<string, object> { ["description"] = "Unauthorized" }
                        }
                    }
                },
                ["/api/weather/favorites/{id}"] = new Dictionary<string, object>
                {
                    ["delete"] = new Dictionary<string, object>
                    {
                        ["tags"] = new[] { "Weather" },
                        ["summary"] = "Remove favorite city",
                        ["security"] = new[] { new Dictionary<string, object[]> { ["Bearer"] = new object[0] } },
                        ["parameters"] = new[]
                        {
                            new Dictionary<string, object>
                            {
                                ["name"] = "id",
                                ["in"] = "path",
                                ["required"] = true,
                                ["schema"] = new Dictionary<string, object> { ["type"] = "string", ["format"] = "uuid" }
                            }
                        },
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["204"] = new Dictionary<string, object> { ["description"] = "Favorite removed" },
                            ["404"] = new Dictionary<string, object> { ["description"] = "Favorite not found" },
                            ["401"] = new Dictionary<string, object> { ["description"] = "Unauthorized" }
                        }
                    }
                },
                ["/api/countries/{name}"] = new Dictionary<string, object>
                {
                    ["get"] = new Dictionary<string, object>
                    {
                        ["tags"] = new[] { "Countries" },
                        ["summary"] = "Get country by name",
                        ["parameters"] = new[]
                        {
                            new Dictionary<string, object>
                            {
                                ["name"] = "name",
                                ["in"] = "path",
                                ["required"] = true,
                                ["schema"] = new Dictionary<string, object> { ["type"] = "string" }
                            }
                        },
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["200"] = new Dictionary<string, object> { ["description"] = "Country data retrieved" },
                            ["404"] = new Dictionary<string, object> { ["description"] = "Country not found" }
                        }
                    }
                },
                ["/api/countries/favorites"] = new Dictionary<string, object>
                {
                    ["get"] = new Dictionary<string, object>
                    {
                        ["tags"] = new[] { "Countries" },
                        ["summary"] = "Get favorite countries",
                        ["security"] = new[] { new Dictionary<string, object[]> { ["Bearer"] = new object[0] } },
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["200"] = new Dictionary<string, object> { ["description"] = "List of favorite countries" },
                            ["401"] = new Dictionary<string, object> { ["description"] = "Unauthorized" }
                        }
                    },
                    ["post"] = new Dictionary<string, object>
                    {
                        ["tags"] = new[] { "Countries" },
                        ["summary"] = "Add favorite country",
                        ["security"] = new[] { new Dictionary<string, object[]> { ["Bearer"] = new object[0] } },
                        ["requestBody"] = new Dictionary<string, object>
                        {
                            ["required"] = true,
                            ["content"] = new Dictionary<string, object>
                            {
                                ["application/json"] = new Dictionary<string, object>
                                {
                                    ["schema"] = new Dictionary<string, object>
                                    {
                                        ["type"] = "object",
                                        ["properties"] = new Dictionary<string, object>
                                        {
                                            ["countryName"] = new Dictionary<string, object> { ["type"] = "string" }
                                        }
                                    }
                                }
                            }
                        },
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["201"] = new Dictionary<string, object> { ["description"] = "Favorite added" },
                            ["401"] = new Dictionary<string, object> { ["description"] = "Unauthorized" }
                        }
                    }
                },
                ["/api/countries/favorites/{id}"] = new Dictionary<string, object>
                {
                    ["delete"] = new Dictionary<string, object>
                    {
                        ["tags"] = new[] { "Countries" },
                        ["summary"] = "Remove favorite country",
                        ["security"] = new[] { new Dictionary<string, object[]> { ["Bearer"] = new object[0] } },
                        ["parameters"] = new[]
                        {
                            new Dictionary<string, object>
                            {
                                ["name"] = "id",
                                ["in"] = "path",
                                ["required"] = true,
                                ["schema"] = new Dictionary<string, object> { ["type"] = "string", ["format"] = "uuid" }
                            }
                        },
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["204"] = new Dictionary<string, object> { ["description"] = "Favorite removed" },
                            ["404"] = new Dictionary<string, object> { ["description"] = "Favorite not found" },
                            ["401"] = new Dictionary<string, object> { ["description"] = "Unauthorized" }
                        }
                    }
                }
            },
            ["components"] = new Dictionary<string, object>
            {
                ["securitySchemes"] = new Dictionary<string, object>
                {
                    ["Bearer"] = new Dictionary<string, object>
                    {
                        ["type"] = "http",
                        ["scheme"] = "bearer",
                        ["bearerFormat"] = "JWT"
                    }
                }
            }
        };

        return JsonSerializer.Serialize(swagger, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
    
    private string GetSwaggerHtml()
    {
        var apiUrl = Environment.GetEnvironmentVariable("API_URL") ?? "https://3ei1klgibg.execute-api.us-east-1.amazonaws.com/prod";
        
        return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>GloboClima API - Swagger UI</title>
    <link rel=""stylesheet"" href=""https://unpkg.com/swagger-ui-dist@5.11.0/swagger-ui.css"">
    <style>
        html { box-sizing: border-box; overflow: -moz-scrollbars-vertical; overflow-y: scroll; }
        *, *:before, *:after { box-sizing: inherit; }
        body { margin:0; background: #fafafa; }
    </style>
</head>
<body>
    <div id=""swagger-ui""></div>
    <script src=""https://unpkg.com/swagger-ui-dist@5.11.0/swagger-ui-bundle.js""></script>
    <script src=""https://unpkg.com/swagger-ui-dist@5.11.0/swagger-ui-standalone-preset.js""></script>
    <script>
        window.onload = function() {
            window.ui = SwaggerUIBundle({
                url: """ + apiUrl + @"/swagger"",
                dom_id: '#swagger-ui',
                deepLinking: true,
                presets: [
                    SwaggerUIBundle.presets.apis,
                    SwaggerUIStandalonePreset
                ],
                plugins: [
                    SwaggerUIBundle.plugins.DownloadUrl
                ],
                layout: ""StandaloneLayout""
            });
        };
    </script>
</body>
</html>";
    }
}