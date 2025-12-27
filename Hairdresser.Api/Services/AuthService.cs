using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Hairdresser.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<ClaimsPrincipal?> ValidateCredentialsAsync(string username, string password)
        {
            var adminUsername = _configuration["AdminCredentials:Username"];
            var adminPassword = _configuration["AdminCredentials:Password"];

            if (username == adminUsername && password == adminPassword)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(claimsIdentity);

                _logger.LogInformation("User {Username} authenticated successfully", username);
                return Task.FromResult<ClaimsPrincipal?>(principal);
            }

            _logger.LogWarning("Failed authentication attempt for user {Username}", username);
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
    }
}

