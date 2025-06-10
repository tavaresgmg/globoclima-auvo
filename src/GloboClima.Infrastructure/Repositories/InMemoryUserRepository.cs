using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces.Repositories;
using System.Collections.Concurrent;

namespace GloboClima.Infrastructure.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly ConcurrentDictionary<string, User> _usersByEmail = new();

    public Task<User> CreateAsync(User user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        
        _users[user.Id] = user;
        _usersByEmail[user.Email.ToLower()] = user;
        
        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        _usersByEmail.TryGetValue(email.ToLower(), out var user);
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user)
    {
        if (_users.ContainsKey(user.Id))
        {
            _users[user.Id] = user;
            _usersByEmail[user.Email.ToLower()] = user;
        }
        return Task.FromResult(user);
    }

    public Task DeleteAsync(Guid id)
    {
        if (_users.TryRemove(id, out var user))
        {
            _usersByEmail.TryRemove(user.Email.ToLower(), out _);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string email)
    {
        return Task.FromResult(_usersByEmail.ContainsKey(email.ToLower()));
    }
}