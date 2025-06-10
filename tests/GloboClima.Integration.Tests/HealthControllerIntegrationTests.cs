using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GloboClima.Integration.Tests;

public class HealthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResponse = JsonSerializer.Deserialize<JsonElement>(content);
        
        healthResponse.GetProperty("status").GetString().Should().Be("healthy");
        healthResponse.GetProperty("service").GetString().Should().Be("GloboClima.Api");
    }
}