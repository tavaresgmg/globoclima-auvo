using System.Security.Claims;
using GloboClima.Application.DTOs.Country;
using GloboClima.Application.Interfaces;
using GloboClima.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GloboClima.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly ICountryUseCase _countryUseCase;

    public CountriesController(ICountryService countryService, ICountryUseCase countryUseCase)
    {
        _countryService = countryService;
        _countryUseCase = countryUseCase;
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetCountryByCode(string code)
    {
        var result = await _countryService.GetCountryByCodeAsync(code);
        
        if (result == null)
            return NotFound(new { message = "Country not found" });
            
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchCountries([FromQuery] string name)
    {
        var result = await _countryService.SearchCountriesByNameAsync(name);
        return Ok(result);
    }

    [HttpGet("favorites")]
    [Authorize]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _countryUseCase.GetFavoritesAsync(userId);
        return Ok(result);
    }

    [HttpPost("favorites")]
    [Authorize]
    public async Task<IActionResult> AddFavorite([FromBody] CountryFavoriteRequestDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _countryUseCase.AddFavoriteAsync(userId, request);
        
        if (result == null)
            return BadRequest(new { message = "Country already in favorites" });
            
        return Created($"api/countries/favorites/{result.Id}", result);
    }

    [HttpDelete("favorites/{id}")]
    [Authorize]
    public async Task<IActionResult> RemoveFavorite(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _countryUseCase.RemoveFavoriteAsync(userId, id);
        
        if (!result)
            return NotFound(new { message = "Favorite not found" });
            
        return NoContent();
    }
}