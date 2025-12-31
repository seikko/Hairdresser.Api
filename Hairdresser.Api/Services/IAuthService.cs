using System.Security.Claims;

namespace Hairdresser.Api.Services;

public interface IAuthService
{
    Task<ClaimsPrincipal?> ValidateCredentialsAsync(string username, string password);
}