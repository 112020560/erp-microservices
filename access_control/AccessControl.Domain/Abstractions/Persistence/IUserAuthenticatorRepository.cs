using System;
using AccessControl.Domain.Entities;

namespace AccessControl.Domain.Abstractions.Persistence;


public interface IUserAuthenticatorRepository
{
    Task<UserAuthenticator?> GetByUserId(Guid userId);
    Task Add(UserAuthenticator userAuthenticator);
    Task UpdateCredentialsAsync(Guid userId, string newCredentials);
}
