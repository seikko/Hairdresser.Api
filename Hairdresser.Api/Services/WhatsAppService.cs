using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Hairdresser.Api.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly string _phoneNumberId;
        private readonly string _accessToken;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _phoneNumberId = _configuration["WhatsApp:PhoneNumberId"] ?? throw new InvalidOperationException("WhatsApp:PhoneNumberId not configured");
            _accessToken = _configuration["WhatsApp:AccessToken"] ?? throw new InvalidOperationException("WhatsApp:AccessToken not configured");

            _httpClient.BaseAddress = new Uri("https://graph.facebook.com/v18.0/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<bool> SendTextMessageAsync(string to, string message)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending WhatsApp message to {To}", to);
                return false;
            }
        }

        public async Task<bool> SendInteractiveButtonsAsync(string to, string bodyText, List<(string id, string title)> buttons)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending interactive buttons to {To}", to);
                return false;
            }
        }

        public async Task<bool> SendInteractiveListAsync(string to, string bodyText, string buttonText, List<(string id, string title, string? description)> rows)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending interactive list to {To}", to);
                return false;
            }
        }

        public async Task MarkMessageAsReadAsync(string messageId)
        {
            try
            {
                var payload = new
                {
                    messaging_product = "whatsapp",
                    status = "read",
                    message_id = messageId
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark message as read: {MessageId}", messageId);
            }
        }
    }
}

