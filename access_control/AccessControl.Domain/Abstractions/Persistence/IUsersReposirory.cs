using System;
using AccessControl.Domain.Entities;

namespace AccessControl.Domain.Abstractions.Persistence;

public interface IUsersReposirory
{
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateEmailVerifiedAsync(Guid userId, bool isEmailVerified, CancellationToken cancellationToken = default);
    Task UpdateUserStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);
}