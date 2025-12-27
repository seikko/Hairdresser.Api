using System.Text.Json.Serialization;

namespace Hairdresser.Api.Models
{
    public class WhatsAppWebhookPayload
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = null!;

        [JsonPropertyName("entry")]
        public List<WebhookEntry> Entry { get; set; } = new();
    }

    public class WebhookEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("changes")]
        public List<WebhookChange> Changes { get; set; } = new();
    }

    public class WebhookChange
    {
        [JsonPropertyName("value")]
        public WebhookValue Value { get; set; } = null!;

        [JsonPropertyName("field")]
        public string Field { get; set; } = null!;
    }

    public class WebhookValue
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = null!;

        [JsonPropertyName("metadata")]
        public WebhookMetadata Metadata { get; set; } = null!;

        [JsonPropertyName("contacts")]
        public List<WebhookContact>? Contacts { get; set; }

        [JsonPropertyName("messages")]
        public List<WebhookMessage>? Messages { get; set; }

        [JsonPropertyName("statuses")]
        public List<WebhookStatus>? Statuses { get; set; }
    }

    public class WebhookMetadata
    {
        [JsonPropertyName("display_phone_number")]
        public string DisplayPhoneNumber { get; set; } = null!;

        [JsonPropertyName("phone_number_id")]
        public string PhoneNumberId { get; set; } = null!;
    }

    public class WebhookContact
    {
        [JsonPropertyName("profile")]
        public WebhookProfile Profile { get; set; } = null!;

        [JsonPropertyName("wa_id")]
        public string WaId { get; set; } = null!;
    }

    public class WebhookProfile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }

    public class WebhookMessage
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = null!;

        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = null!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("text")]
        public WebhookText? Text { get; set; }

        [JsonPropertyName("interactive")]
        public WebhookInteractive? Interactive { get; set; }
    }

    public class WebhookText
    {
        [JsonPropertyName("body")]
        public string Body { get; set; } = null!;
    }

    public class WebhookInteractive
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("button_reply")]
        public WebhookButtonReply? ButtonReply { get; set; }

        [JsonPropertyName("list_reply")]
        public WebhookListReply? ListReply { get; set; }
    }

    public class WebhookButtonReply
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
    }

    public class WebhookListReply
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class WebhookStatus
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = null!;

        [JsonPropertyName("recipient_id")]
        public string RecipientId { get; set; } = null!;
    }
}

