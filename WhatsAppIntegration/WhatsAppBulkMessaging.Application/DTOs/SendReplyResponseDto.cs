
namespace WhatsAppBulkMessaging.Application.DTOs;

public class SendReplyResponseDto
{
    public string PhoneNumber { get; set; } = string.Empty;

    public string MessageType { get; set; } = string.Empty;

    public string? TextMessageId { get; set; }

    public string? MediaMessageId { get; set; }

    public string? MediaId { get; set; }

    public string? MimeType { get; set; }

    public string? FileName { get; set; }

    public string Message { get; set; } = string.Empty;
}
