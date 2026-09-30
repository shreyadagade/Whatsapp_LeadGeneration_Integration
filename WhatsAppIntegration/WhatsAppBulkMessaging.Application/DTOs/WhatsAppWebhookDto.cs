using System.Text.Json.Serialization;

namespace WhatsAppBulkMessaging.Application.DTOs;

public class WhatsAppWebhookDto
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("entry")]
    public List<WhatsAppWebhookEntryDto> Entry { get; set; } = [];
}

public class WhatsAppWebhookEntryDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("changes")]
    public List<WhatsAppWebhookChangeDto> Changes { get; set; } = [];
}

public class WhatsAppWebhookChangeDto
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("value")]
    public WhatsAppWebhookValueDto? Value { get; set; }
}

public class WhatsAppWebhookValueDto
{
    [JsonPropertyName("messaging_product")]
    public string? MessagingProduct { get; set; }

    [JsonPropertyName("metadata")]
    public WhatsAppWebhookMetadataDto? Metadata { get; set; }

    [JsonPropertyName("statuses")]
    public List<WhatsAppWebhookStatusDto> Statuses { get; set; } = [];

    [JsonPropertyName("messages")]
    public List<WhatsAppWebhookMessageDto> Messages { get; set; } = [];
}

public class WhatsAppWebhookMetadataDto
{
    [JsonPropertyName("display_phone_number")]
    public string? DisplayPhoneNumber { get; set; }

    [JsonPropertyName("phone_number_id")]
    public string? PhoneNumberId { get; set; }
}

public class WhatsAppWebhookStatusDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("recipient_id")]
    public string? RecipientId { get; set; }
}

public class WhatsAppWebhookMessageDto
{
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public WhatsAppWebhookTextDto? Text { get; set; }
}

public class WhatsAppWebhookTextDto
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}