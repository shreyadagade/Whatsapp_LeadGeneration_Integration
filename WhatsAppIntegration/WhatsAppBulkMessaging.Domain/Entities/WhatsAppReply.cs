
namespace WhatsAppBulkMessaging.Domain.Entities;

public class WhatsAppReply
{
    public long Id { get; set; }

    public long? WhatsAppMessageId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string ReplyMessage { get; set; } = null!;

    public string MetaMessageId { get; set; } = null!;

    public DateTime ReplyReceivedAt { get; set; }
}
