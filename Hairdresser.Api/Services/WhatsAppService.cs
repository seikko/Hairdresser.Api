using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hairdresser.Api.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppService> _logger;

    private readonly string _phoneNumberId;
    private readonly string _appId;
    private readonly string _appSecret;
    private readonly string _longLivedToken;

    private string _currentAccessToken = "";
    private DateTime _tokenExpiry = DateTime.MinValue;

    public WhatsAppService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _phoneNumberId = _configuration["WhatsApp:PhoneNumberId"] ?? throw new InvalidOperationException("WhatsApp:PhoneNumberId not configured");
        _appId = _configuration["WhatsApp:AppId"] ?? throw new InvalidOperationException("WhatsApp:AppId not configured");
        _appSecret = _configuration["WhatsApp:AppSecret"] ?? throw new InvalidOperationException("WhatsApp:AppSecret not configured");
        _longLivedToken = _configuration["WhatsApp:AccessToken"] ?? throw new InvalidOperationException("WhatsApp:AccessToken not configured");

        _httpClient.BaseAddress = new Uri("https://graph.facebook.com/v18.0/");
    }

    private async Task EnsureAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_currentAccessToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            return;

        _logger.LogInformation("Refreshing WhatsApp access token...");

        var url = $"https://graph.facebook.com/v18.0/oauth/access_token" +
                  $"?grant_type=fb_exchange_token" +
                  $"&client_id={_appId}" +
                  $"&client_secret={_appSecret}" +
                  $"&fb_exchange_token={_longLivedToken}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to refresh access token: {Error}", error);
            throw new Exception("Failed to refresh WhatsApp access token");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        _currentAccessToken = doc.RootElement.GetProperty("access_token").GetString()!;
        var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);

        _logger.LogInformation("Access token refreshed. Expires at {Expiry}", _tokenExpiry);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
    }

    public async Task<bool> SendTextMessageAsync(string to, string message)
    {
        await EnsureAccessTokenAsync();
        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = to,
            type = "text",
            text = new { body = message }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send WhatsApp message. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            return false;
        }

        _logger.LogInformation("Successfully sent WhatsApp message to {To}", to);
        return true;
    }

    public async Task<bool> SendInteractiveButtonsAsync(string to, string bodyText, List<(string id, string title)> buttons)
    {
        await EnsureAccessTokenAsync();

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = to,
            type = "interactive",
            interactive = new
            {
                type = "button",
                body = new { text = bodyText },
                action = new
                {
                    buttons = buttons.Select(b => new
                    {
                        type = "reply",
                        reply = new { id = b.id, title = b.title }
                    }).ToList()
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send interactive buttons. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            return false;
        }

        _logger.LogInformation("Successfully sent interactive buttons to {To}", to);
        return true;
    }

    public async Task<bool> SendInteractiveListAsync(string to, string bodyText, string buttonText, List<(string id, string title, string? description)> rows)
    {
        await EnsureAccessTokenAsync();

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = to,
            type = "interactive",
            interactive = new
            {
                type = "list",
                body = new { text = bodyText },
                action = new
                {
                    button = buttonText,
                    sections = new[]
                    {
                        new
                        {
                            title = "Seçenekler",
                            rows = rows.Select(r => new
                            {
                                id = r.id,
                                title = r.title,
                                description = r.description
                            }).ToList()
                        }
                    }
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send interactive list. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            return false;
        }

        _logger.LogInformation("Successfully sent interactive list to {To}", to);
        return true;
    }

    public async Task MarkMessageAsReadAsync(string messageId)
    {
        await EnsureAccessTokenAsync();

        var payload = new
        {
            messaging_product = "whatsapp",
            status = "read",
            message_id = messageId
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);
    }
}