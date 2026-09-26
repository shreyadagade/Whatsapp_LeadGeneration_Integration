using WhatsAppBulkMessaging.Application.DTOs;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppMetaService
{
    Task<MetaTemplateResponseDto> GetTemplatesAsync();
}