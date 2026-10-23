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

    [JsonPropertyName("errors")]
    public List<WhatsAppWebhookErrorDto> Errors { get; set; } = [];
}

public class WhatsAppWebhookErrorDto
{
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error_data")]
    public WhatsAppWebhookErrorDataDto? ErrorData { get; set; }
}

public class WhatsAppWebhookErrorDataDto
{
    [JsonPropertyName("details")]
    public string? Details { get; set; }
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

    [JsonPropertyName("image")]
    public WhatsAppWebhookMediaDto? Image { get; set; }

    [JsonPropertyName("video")]
    public WhatsAppWebhookMediaDto? Video { get; set; }

    [JsonPropertyName("audio")]
    public WhatsAppWebhookMediaDto? Audio { get; set; }

    [JsonPropertyName("document")]
    public WhatsAppWebhookMediaDto? Document { get; set; }

    [JsonPropertyName("context")]
    public WhatsAppWebhookContextDto? Context { get; set; }
}

public class WhatsAppWebhookMediaDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("filename")]
    public string? FileName { get; set; }

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }
}

public class WhatsAppWebhookContextDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class WhatsAppWebhookTextDto
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}