
using WhatsAppBulkMessaging.Application.DTOs;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppMessageHistoryService
{
    Task<List<WhatsAppMessageResponseDto>> GetAllMessagesAsync();

    Task<List<WhatsAppMessageResponseDto>>
        GetMessagesByPhoneNumberAsync(string phoneNumber);
}
