using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Hairdresser.Api.Services;

public class AuthService(IConfiguration configuration, ILogger<AuthService> logger) : IAuthService
{
    public Task<ClaimsPrincipal?> ValidateCredentialsAsync(string username, string password)
    {
        var adminUsername = configuration["AdminCredentials:Username"];
        var adminPassword = configuration["AdminCredentials:Password"];

        if (username == adminUsername && password == adminPassword)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);

            logger.LogInformation("User {Username} authenticated successfully", username);
            return Task.FromResult<ClaimsPrincipal?>(principal);
        }

        logger.LogWarning("Failed authentication attempt for user {Username}", username);
        return Task.FromResult<ClaimsPrincipal?>(null);
    }
}