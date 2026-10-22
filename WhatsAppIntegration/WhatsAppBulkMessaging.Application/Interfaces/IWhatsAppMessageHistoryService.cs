
using WhatsAppBulkMessaging.Application.DTOs;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppMessageHistoryService
{
    Task<List<WhatsAppMessageResponseDto>> GetAllMessagesAsync();
    Task<List<WhatsAppMessageResponseDto>>GetMessagesByPhoneNumberAsync(string phoneNumber);
    Task<List<WhatsAppChatMessageResponseDto>>GetChatHistoryByPhoneNumberAsync(string phoneNumber);
    Task<List<WhatsAppChatListResponseDto>> GetChatListAsync();

    Task MarkChatAsReadAsync(string phoneNumber);
}
