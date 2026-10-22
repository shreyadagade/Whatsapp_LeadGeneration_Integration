
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Infrastructure.Repositories;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class WhatsAppMessageHistoryService: IWhatsAppMessageHistoryService
{
    private readonly IWhatsAppMessageRepository _messageRepository;
    private readonly WhatsAppChatMessageRepository _chatRepository;

    public WhatsAppMessageHistoryService(IWhatsAppMessageRepository messageRepository,
            WhatsAppChatMessageRepository chatRepository)
    {
        _messageRepository = messageRepository;
        _chatRepository = chatRepository;
    }

    public async Task<List<WhatsAppMessageResponseDto>>GetAllMessagesAsync()
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
            UpdatedAt = message.UpdatedAt
        }).ToList();
    }

    public async Task<List<WhatsAppMessageResponseDto>>GetMessagesByPhoneNumberAsync(string phoneNumber)
    {
        var messages = await _messageRepository.GetByPhoneNumberAsync(phoneNumber);

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
            UpdatedAt = message.UpdatedAt
        }).ToList();
    }

    public async Task<List<WhatsAppChatMessageResponseDto>>GetChatHistoryByPhoneNumberAsync(string phoneNumber)
    {
        var messages = await _chatRepository.GetByPhoneNumberAsync(phoneNumber);

        return messages.Select(message => new WhatsAppChatMessageResponseDto
        {
            Id = message.Id,
            PhoneNumber = message.PhoneNumber,
            Direction = message.Direction,
            MessageType = message.MessageType,
            MessageText = message.MessageText,
            MediaId = message.MediaId,
            MimeType = message.MimeType,
            FileName = message.FileName,
            MetaMessageId = message.MetaMessageId,
            Status = message.Status,
            ErrorMessage = message.ErrorMessage,
            CreatedAt = message.CreatedAt,
            SentAt = message.SentAt,
            DeliveredAt = message.DeliveredAt,
            ReadAt = message.ReadAt
        }).ToList();

    }
    public async Task<List<WhatsAppChatListResponseDto>> GetChatListAsync()
    {
        var messages = await _chatRepository.GetAllChatMessagesAsync();

        return messages
            .GroupBy(x => x.PhoneNumber)
            .Select(group =>
            {
                var latestMessage = group
                    .OrderByDescending(x => x.CreatedAt)
                    .First();

                return new WhatsAppChatListResponseDto
                {
                    PhoneNumber = group.Key,
                    LastMessage = latestMessage.MessageText,
                    LastMessageAt = latestMessage.CreatedAt,
                    UnreadCount = group.Count(x =>
                        x.Direction == "Incoming" && !x.IsRead)
                };
            })
            .OrderByDescending(x => x.LastMessageAt)
            .ToList();
    }
    public async Task MarkChatAsReadAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        await _chatRepository.MarkAsReadAsync(phoneNumber);
    }
}
