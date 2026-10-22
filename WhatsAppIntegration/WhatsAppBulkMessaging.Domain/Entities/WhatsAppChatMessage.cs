namespace WhatsAppBulkMessaging.Domain.Entities;

public class WhatsAppChatMessage
{
    public long Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text";
    public string? MessageText { get; set; }
    public string? MediaId { get; set; }
    public string? MimeType { get; set; }
    public string? FileName { get; set; }
    public string? MetaMessageId { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
}