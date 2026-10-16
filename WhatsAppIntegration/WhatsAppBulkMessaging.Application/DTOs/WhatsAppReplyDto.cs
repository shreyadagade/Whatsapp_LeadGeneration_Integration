
namespace WhatsAppBulkMessaging.Application.DTOs;

public class WhatsAppReplyDto
{
    public long? WhatsAppMessageId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string ReplyMessage { get; set; } = null!;

    public string MetaMessageId { get; set; } = null!;

    public DateTime ReplyReceivedAt { get; set; }
}
