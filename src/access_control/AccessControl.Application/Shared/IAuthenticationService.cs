using System;

namespace AccessControl.Application.Shared;

public interface IAuthenticationService
{
    Task<(string accessToken, string refreshToken)> RequestTokenUsingPasswordGrantAsync(string username, string password);
    Task<(string accessToken, string refreshToken)> RefreshAccessTokenAsync(string refreshToken);
}
