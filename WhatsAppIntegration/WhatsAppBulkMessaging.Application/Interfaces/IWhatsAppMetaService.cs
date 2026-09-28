using WhatsAppBulkMessaging.Application.DTOs;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppMetaService
{
    Task<MetaTemplateResponseDto> GetTemplatesAsync();
    Task<string> UploadMediaAsync(Stream fileStream, string fileName, string contentType);
}