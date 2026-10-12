

using WhatsAppBulkMessaging.Application.DTOs;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppService
{
    Task<string> SendTemplateMessageAsync(MetaSendMessageRequestDto request);
}