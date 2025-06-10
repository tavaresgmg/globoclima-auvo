using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics;
using System.Security.Claims;

namespace GloboClima.Web.Services;

// This is a server-side AuthenticationStateProvider that uses PersistentComponentState to flow the
// authentication state to the client which is then fixed for the lifetime of the WebAssembly application.
public class PersistentAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;
    private readonly ILocalStorageService _localStorage;
    
    private Task<AuthenticationState>? _authenticationStateTask;

    public PersistentAuthenticationStateProvider(
        PersistentComponentState persistentComponentState,
        ILocalStorageService localStorage)
    {
        _state = persistentComponentState;
        _localStorage = localStorage;
        
        AuthenticationStateChanged += OnAuthenticationStateChanged;
        _subscription = _state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        if (_authenticationStateTask is null)
        {
            throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
        }

        var authenticationState = await _authenticationStateTask;
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var identity = principal.Identity as ClaimsIdentity;
            var name = identity?.FindFirst(ClaimTypes.Name)?.Value;
            var email = identity?.FindFirst(ClaimTypes.Email)?.Value;
            
            if (name != null && email != null)
            {
                _state.PersistAsJson(nameof(UserInfo), new UserInfo
                {
                    Name = name,
                    Email = email,
                });
            }
        }
    }

    public void Dispose()
    {
        _subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }

    private sealed class UserInfo
    {
        public required string Name { get; init; }
        public required string Email { get; init; }
    }
}