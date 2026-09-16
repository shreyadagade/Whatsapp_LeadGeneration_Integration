namespace WhatsAppBulkMessaging.Domain.Entities;

public class WhatsAppMessage
{
    public long Id { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string? MetaMessageId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? FailureReason { get; set; }

    public string? UserFriendlyFailureReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}