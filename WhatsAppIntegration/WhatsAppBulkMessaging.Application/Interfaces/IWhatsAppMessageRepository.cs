using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppMessageRepository
{
    Task AddAsync(WhatsAppMessage message);
    Task UpdateStatusAsync(
        string metaMessageId,
        string status,
        string? failureReason = null);
}