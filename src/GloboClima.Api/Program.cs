using System.Text;
using GloboClima.Application.Interfaces;
using GloboClima.Application.UseCases;
using GloboClima.Domain.Interfaces.Repositories;
using GloboClima.Domain.Interfaces.Services;
using GloboClima.Infrastructure.Repositories;
using GloboClima.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
    builder.Configuration["Jwt:Secret"] ?? 
    throw new InvalidOperationException("JWT Secret not configured. Set JWT_SECRET environment variable.");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // Force HTTPS in production
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "GloboClima",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "GloboClima",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "GloboClima API", 
        Version = "v1.0.0",
        Description = "API REST para consulta de informações climáticas e de países com sistema de favoritos",
        Contact = new OpenApiContact
        {
            Name = "GloboClima Support",
            Email = "support@globoclima.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    
    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Add examples and better descriptions
    c.UseInlineDefinitionsForEnums();
});

// Configure repositories based on environment
if (builder.Environment.IsDevelopment())
{
    // Use in-memory repositories for development
    builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    builder.Services.AddSingleton<IWeatherFavoriteRepository, InMemoryWeatherFavoriteRepository>();
    builder.Services.AddSingleton<ICountryFavoriteRepository, InMemoryCountryFavoriteRepository>();
}
else
{
    // Configure DynamoDB for production
    builder.Services.AddSingleton<DynamoDbContext>();
    
    // Repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IWeatherFavoriteRepository, WeatherFavoriteRepository>();
    builder.Services.AddScoped<ICountryFavoriteRepository, CountryFavoriteRepository>();
}

// Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure HTTP retry policy
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => !msg.IsSuccessStatusCode)
    .WaitAndRetryAsync(
        3,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            // Retry logging handled by Polly
        });

// Configure circuit breaker policy
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        3,
        TimeSpan.FromSeconds(30),
        onBreak: (result, timespan) =>
        {
            // Circuit breaker opened
        },
        onReset: () =>
        {
            // Circuit breaker reset
        });

// Combine policies
var resilientPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

// Configure Weather Service based on environment variable or configuration
var useOpenWeatherMap = Environment.GetEnvironmentVariable("USE_OPENWEATHERMAP") ?? 
    builder.Configuration["WeatherService:UseOpenWeatherMap"];

if (useOpenWeatherMap?.ToLower() == "true")
{
    builder.Services.AddHttpClient<IWeatherService, OpenWeatherMapService>()
        .AddPolicyHandler(resilientPolicy);
}
else
{
    builder.Services.AddHttpClient<IWeatherService, WeatherService>()
        .AddPolicyHandler(resilientPolicy);
}

builder.Services.AddHttpClient<ICountryService, CountryService>()
    .AddPolicyHandler(resilientPolicy);

// Use Cases
builder.Services.AddScoped<IAuthUseCase, AuthUseCase>();
builder.Services.AddScoped<IWeatherUseCase, WeatherUseCase>();
builder.Services.AddScoped<ICountryUseCase, CountryUseCase>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure for AWS Lambda - commented out for local development
// builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GloboClima API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
