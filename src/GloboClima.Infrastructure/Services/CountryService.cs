using System.Text.Json;
using GloboClima.Domain.Interfaces.Services;
using GloboClima.Domain.Models.Country;

namespace GloboClima.Infrastructure.Services;

public class CountryService : ICountryService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://restcountries.com/v3.1";
    private static List<CountryResponse>? _cachedCountries;
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public CountryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CountryResponse?> GetCountryByCodeAsync(string code)
    {
        // Check cache first
        if (_cachedCountries != null && DateTime.UtcNow < _cacheExpiry)
        {
            var cachedCountry = _cachedCountries.FirstOrDefault(c => 
                c.Cca2?.Equals(code, StringComparison.OrdinalIgnoreCase) == true ||
                c.Cca3?.Equals(code, StringComparison.OrdinalIgnoreCase) == true
            );
            
            if (cachedCountry != null)
                return cachedCountry;
        }
        
        try
        {
            var url = $"{_baseUrl}/alpha/{code}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var countries = JsonSerializer.Deserialize<List<CountryResponse>>(json, options);
            var country = countries?.FirstOrDefault();
            
            if (country != null)
            {
                TranslateCountryResponse(country);
            }
            
            return country;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<CountryResponse>> SearchCountriesByNameAsync(string name)
    {
        // If we have cached countries, search locally first for better performance
        if (_cachedCountries != null && DateTime.UtcNow < _cacheExpiry)
        {
            var localResults = _cachedCountries.Where(c => 
                c.Name?.Common?.Contains(name, StringComparison.OrdinalIgnoreCase) == true ||
                c.Name?.Official?.Contains(name, StringComparison.OrdinalIgnoreCase) == true ||
                c.Cca2?.Equals(name, StringComparison.OrdinalIgnoreCase) == true ||
                c.Cca3?.Equals(name, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
            
            if (localResults.Any())
                return localResults;
        }
        
        try
        {
            var url = $"{_baseUrl}/name/{Uri.EscapeDataString(name)}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return new List<CountryResponse>();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var countries = JsonSerializer.Deserialize<List<CountryResponse>>(json, options) ?? new List<CountryResponse>();
            
            foreach (var country in countries)
            {
                TranslateCountryResponse(country);
            }
            
            return countries;
        }
        catch
        {
            return new List<CountryResponse>();
        }
    }

    public async Task<List<CountryResponse>> GetAllCountriesAsync()
    {
        // Check if cache is still valid
        if (_cachedCountries != null && DateTime.UtcNow < _cacheExpiry)
        {
            return _cachedCountries;
        }
        
        try
        {
            var url = $"{_baseUrl}/all";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return new List<CountryResponse>();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var countries = JsonSerializer.Deserialize<List<CountryResponse>>(json, options) ?? new List<CountryResponse>();
            
            foreach (var country in countries)
            {
                TranslateCountryResponse(country);
            }
            
            // Update cache
            _cachedCountries = countries;
            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
            
            return countries;
        }
        catch
        {
            // If there's an error but we have cached data, return it
            if (_cachedCountries != null)
                return _cachedCountries;
                
            return new List<CountryResponse>();
        }
    }
    
    private void TranslateCountryResponse(CountryResponse country)
    {
        if (country == null) return;
        
        // Traduzir nome do país
        if (!string.IsNullOrWhiteSpace(country.Name?.Common))
        {
            country.Name.Common = TranslationService.TranslateCountryName(country.Name.Common);
        }
        
        if (!string.IsNullOrWhiteSpace(country.Name?.Official))
        {
            country.Name.Official = TranslationService.TranslateCountryName(country.Name.Official);
        }
    }
}