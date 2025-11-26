using System;
using AccessControl.Domain.Abstractions.Persistence;
using AccessControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Persistence.Repositories;

public class UsersReposirory: IUsersReposirory
{
    private readonly AppDbContext _context;
    public UsersReposirory(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FindAsync(new object?[] { id }, cancellationToken);
    }
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FindAsync(new object?[] { email }, cancellationToken);
    }

    public async Task UpdateEmailVerifiedAsync(Guid userId, bool isEmailVerified, CancellationToken cancellationToken = default)
    {
        await _context.Users.Where(x => x.Id == userId).ExecuteUpdateAsync(x => x.SetProperty(x => x.IsEmailVerified, isEmailVerified), cancellationToken);
    }

    public async Task UpdateUserStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default)
    {
        await _context.Users.Where(x => x.Id == userId).ExecuteUpdateAsync(x => x.SetProperty(x => x.IsActive, isActive), cancellationToken);
    }
}
