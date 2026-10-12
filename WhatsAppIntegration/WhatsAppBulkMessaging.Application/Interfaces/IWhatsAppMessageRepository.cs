
using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppMessageRepository
{
    Task AddAsync(WhatsAppMessage message);

    Task<WhatsAppMessage?> GetByMetaMessageIdAsync(string metaMessageId);

    Task UpdateAsync(WhatsAppMessage message);

    Task UpdateStatusAsync(string metaMessageId,string status,DateTime? statusTime,
        string? errorMessage = null);
    Task<WhatsAppMessage?> GetLatestByPhoneNumberAsync(string phoneNumber);

    Task UpdateReplyAsync(long messageId,string replyMessage,DateTime replyReceivedAt);

    // Get all messages with status and reply information
    Task<List<WhatsAppMessage>> GetAllAsync();

    // Get messages for a specific customer
    Task<List<WhatsAppMessage>> GetByPhoneNumberAsync(string phoneNumber);
}