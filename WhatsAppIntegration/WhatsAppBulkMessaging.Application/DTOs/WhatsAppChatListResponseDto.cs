namespace WhatsAppBulkMessaging.Application.DTOs;

public class WhatsAppChatListResponseDto
{
    public string PhoneNumber { get; set; } = string.Empty;

    public string? LastMessage { get; set; }

    public DateTime LastMessageAt { get; set; }

    public int UnreadCount { get; set; }
}