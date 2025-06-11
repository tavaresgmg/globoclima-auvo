using Blazored.LocalStorage;
using GloboClima.Application.DTOs.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace GloboClima.Web.Services;

public class AuthService
{
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly HttpClient _httpClient;
    private const string TokenKey = "authToken";
    private const string UserKey = "authUser";

    public AuthService(
        ILocalStorageService localStorage, 
        AuthenticationStateProvider authStateProvider,
        HttpClient httpClient)
    {
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(LoginRequestDto loginRequest)
    {
        try
        {
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("api/auth/login", content);
            
            if (!response.IsSuccessStatusCode)
                return false;

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (authResponse != null)
            {
                await _localStorage.SetItemAsync(TokenKey, authResponse.Token);
                await _localStorage.SetItemAsync(UserKey, authResponse);
                
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(authResponse.Token);
                
                return true;
            }
        }
        catch (Exception)
        {
            // Login error - handled by return false
        }
        
        return false;
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto registerRequest)
    {
        try
        {
            var json = JsonSerializer.Serialize(registerRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("api/auth/register", content);
            
            if (!response.IsSuccessStatusCode)
                return false;

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (authResponse != null)
            {
                await _localStorage.SetItemAsync(TokenKey, authResponse.Token);
                await _localStorage.SetItemAsync(UserKey, authResponse);
                
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(authResponse.Token);
                
                return true;
            }
        }
        catch (Exception)
        {
            // Register error - handled by return false
        }
        
        return false;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        await _localStorage.RemoveItemAsync(UserKey);
        
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(TokenKey);
    }

    public async Task<AuthResponseDto?> GetCurrentUserAsync()
    {
        return await _localStorage.GetItemAsync<AuthResponseDto>(UserKey);
    }
}