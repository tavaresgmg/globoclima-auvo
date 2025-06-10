using System.Text.Json;
using GloboClima.Domain.Interfaces.Services;
using GloboClima.Domain.Models.Country;

namespace GloboClima.Infrastructure.Services;

public class CountryService : ICountryService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://restcountries.com/v3.1";

    public CountryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CountryResponse?> GetCountryByCodeAsync(string code)
    {
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
            
            return countries;
        }
        catch
        {
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