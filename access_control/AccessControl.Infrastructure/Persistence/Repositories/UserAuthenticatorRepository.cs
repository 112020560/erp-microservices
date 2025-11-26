using System;
using AccessControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Persistence.Repositories;

public class UserAuthenticatorRepository
{
    private readonly AppDbContext _context;
    public UserAuthenticatorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserAuthenticator?> GetByUserId(Guid userId)
    {
        return await _context.UserAuthenticators.FindAsync(userId);
    }

    public async Task Add(UserAuthenticator userAuthenticator)
    {
        await _context.UserAuthenticators.AddAsync(userAuthenticator);
    }

    public async Task UpdateCredentialsAsync(Guid userId, string newCredentials)
    {
        await _context.UserAuthenticators
            .Where(ua => ua.UserId == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(ua => ua.Credential, newCredentials));
    }
}
