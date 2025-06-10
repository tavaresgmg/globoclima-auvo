using GloboClima.Domain.Models.Country;

namespace GloboClima.Domain.Interfaces.Services;

public interface ICountryService
{
    Task<CountryResponse?> GetCountryByCodeAsync(string code);
    Task<List<CountryResponse>> SearchCountriesByNameAsync(string name);
    Task<List<CountryResponse>> GetAllCountriesAsync();
}