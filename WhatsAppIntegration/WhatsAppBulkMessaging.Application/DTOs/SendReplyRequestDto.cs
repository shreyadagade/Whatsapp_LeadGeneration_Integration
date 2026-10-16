namespace WhatsAppBulkMessaging.Application.DTOs;

public class SendReplyRequestDto
{
    public string PhoneNumber { get; set; } = string.Empty;

    public string MessageType { get; set; } = string.Empty;

    public string? Text { get; set; }

    public string? Caption { get; set; }

    public Stream? FileStream { get; set; }

    public string? FileName { get; set; }

    public string? ContentType { get; set; }
}