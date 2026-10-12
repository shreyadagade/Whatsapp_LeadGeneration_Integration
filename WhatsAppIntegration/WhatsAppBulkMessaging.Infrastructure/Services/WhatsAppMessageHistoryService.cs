
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class WhatsAppMessageHistoryService: IWhatsAppMessageHistoryService
{
    private readonly IWhatsAppMessageRepository _messageRepository;

    public WhatsAppMessageHistoryService(
        IWhatsAppMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<List<WhatsAppMessageResponseDto>>
        GetAllMessagesAsync()
    {
        var messages = await _messageRepository.GetAllAsync();

        return messages.Select(message => new WhatsAppMessageResponseDto
        {
            Id = message.Id,
            PhoneNumber = message.PhoneNumber,
            CandidateName = message.CandidateName,
            WhatsAppTemplateId = message.WhatsAppTemplateId,
            Status = message.Status,
            MetaMessageId = message.MetaMessageId,
            ErrorMessage = message.ErrorMessage,
            RetryCount = message.RetryCount,
            CreatedAt = message.CreatedAt,
            SentAt = message.SentAt,
            DeliveredAt = message.DeliveredAt,
            ReadAt = message.ReadAt,
            FailedAt = message.FailedAt,
            UpdatedAt = message.UpdatedAt,
            ReplyMessage = message.ReplyMessage,
            ReplyReceivedAt = message.ReplyReceivedAt
        }).ToList();
    }

    public async Task<List<WhatsAppMessageResponseDto>>
        GetMessagesByPhoneNumberAsync(string phoneNumber)
    {
        var messages =
            await _messageRepository.GetByPhoneNumberAsync(phoneNumber);

        return messages.Select(message => new WhatsAppMessageResponseDto
        {
            Id = message.Id,
            PhoneNumber = message.PhoneNumber,
            CandidateName = message.CandidateName,
            WhatsAppTemplateId = message.WhatsAppTemplateId,
            Status = message.Status,
            MetaMessageId = message.MetaMessageId,
            ErrorMessage = message.ErrorMessage,
            RetryCount = message.RetryCount,
            CreatedAt = message.CreatedAt,
            SentAt = message.SentAt,
            DeliveredAt = message.DeliveredAt,
            ReadAt = message.ReadAt,
            FailedAt = message.FailedAt,
            UpdatedAt = message.UpdatedAt,
            ReplyMessage = message.ReplyMessage,
            ReplyReceivedAt = message.ReplyReceivedAt
        }).ToList();
    }
}
