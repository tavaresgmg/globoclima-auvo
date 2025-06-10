using System.Security.Claims;
using GloboClima.Application.DTOs.Weather;
using GloboClima.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GloboClima.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherUseCase _weatherUseCase;

    public WeatherController(IWeatherUseCase weatherUseCase)
    {
        _weatherUseCase = weatherUseCase;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> GetWeatherByCity(string city)
    {
        var result = await _weatherUseCase.GetWeatherByCityAsync(city);
        
        if (result == null)
            return NotFound(new { message = "City not found" });
            
        return Ok(result);
    }

    [HttpGet("favorites")]
    [Authorize]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _weatherUseCase.GetFavoritesAsync(userId);
        return Ok(result);
    }

    [HttpPost("favorites")]
    [Authorize]
    public async Task<IActionResult> AddFavorite([FromBody] WeatherFavoriteRequestDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _weatherUseCase.AddFavoriteAsync(userId, request);
        
        if (result == null)
            return BadRequest(new { message = "City already in favorites" });
            
        return Created($"api/weather/favorites/{result.Id}", result);
    }

    [HttpDelete("favorites/{id}")]
    [Authorize]
    public async Task<IActionResult> RemoveFavorite(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _weatherUseCase.RemoveFavoriteAsync(userId, id);
        
        if (!result)
            return NotFound(new { message = "Favorite not found" });
            
        return NoContent();
    }
}