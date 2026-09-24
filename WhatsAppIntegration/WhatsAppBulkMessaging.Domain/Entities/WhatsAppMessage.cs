namespace WhatsAppBulkMessaging.Domain.Entities;

public class WhatsAppMessage
{
    public long Id { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string? CandidateName { get; set; }

    public int WhatsAppTemplateId { get; set; }

    public WhatsAppTemplate WhatsAppTemplate { get; set; } = null!;

    // Current message status
    public string Status { get; set; } = "Pending";

    public string? MetaMessageId { get; set; }

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SentAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}