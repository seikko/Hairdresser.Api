using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hairdresser.Api.Models;
using Hairdresser.Api.Services;

namespace BookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Webhook must be accessible without authentication
    public class WebhookController : ControllerBase
    {
        private readonly IMessageHandler _messageHandler;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            IMessageHandler messageHandler,
            IWhatsAppService whatsAppService,
            IConfiguration configuration,
            ILogger<WebhookController> logger)
        {
            _messageHandler = messageHandler;
            _whatsAppService = whatsAppService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Webhook verification endpoint (GET)
        /// Meta will call this to verify your webhook
        /// </summary>
        [HttpGet]
        public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string token,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            _logger.LogInformation("Webhook verification requested. Mode: {Mode}, Token: {Token}", mode, token);

            var verifyToken = _configuration["WhatsApp:VerifyToken"];

            if (mode == "subscribe" && token == verifyToken)
            {
                _logger.LogInformation("Webhook verified successfully");
                return Ok(challenge);
            }

            _logger.LogWarning("Webhook verification failed. Invalid token. Expected: {Expected}, Received: {Received}",
                verifyToken, token);
            return StatusCode(403, "Forbidden: Invalid verify token");
        }

        /// <summary>
        /// Webhook endpoint to receive messages (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook([FromBody] WhatsAppWebhookPayload payload)
        {
            try
            {
                _logger.LogInformation("Received webhook payload");

                if (payload.Object != "whatsapp_business_account")
                {
                    _logger.LogWarning("Received non-whatsapp payload: {Object}", payload.Object);
                    return Ok();
                }

                foreach (var entry in payload.Entry)
                {
                    foreach (var change in entry.Changes)
                    {
                        if (change.Field != "messages")
                            continue;

                        var value = change.Value;

                        if (value.Messages != null)
                        {
                            foreach (var message in value.Messages)
                            {
                                await _whatsAppService.MarkMessageAsReadAsync(message.Id);

                                string? senderName = null;
                                if (value.Contacts != null)
                                {
                                    var contact = value.Contacts.FirstOrDefault(c => c.WaId == message.From);
                                    senderName = contact?.Profile?.Name;
                                }

                                if (message.Type == "text" && message.Text != null)
                                {
                                    await _messageHandler.HandleIncomingMessageAsync(
                                        message.From,
                                        message.Text.Body,
                                        senderName
                                    );
                                }
                                else if (message.Type == "interactive" && message.Interactive != null)
                                {
                                    var interactive = message.Interactive;
                                    if (interactive.ButtonReply != null)
                                    {
                                        await _messageHandler.HandleInteractiveReplyAsync(
                                            message.From,
                                            interactive.ButtonReply.Id,
                                            interactive.ButtonReply.Title
                                        );
                                    }
                                    else if (interactive.ListReply != null)
                                    {
                                        await _messageHandler.HandleInteractiveReplyAsync(
                                            message.From,
                                            interactive.ListReply.Id,
                                            interactive.ListReply.Title
                                        );
                                    }
                                }
                            }
                        }

                        if (value.Statuses != null)
                        {
                            foreach (var status in value.Statuses)
                            {
                                _logger.LogInformation("Message status update: {MessageId} - {Status}", status.Id,
                                    status.Status);
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return Ok();
            }
        }
    }
}

