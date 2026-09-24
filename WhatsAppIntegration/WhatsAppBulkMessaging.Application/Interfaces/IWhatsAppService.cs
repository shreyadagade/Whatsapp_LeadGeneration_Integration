using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppService
{
    //Task<string> SendTemplateMessageAsync(
    //    string phoneNumber,
    //    string templateName,
    //    string? headerImageMediaId = null);

    Task<string> SendTemplateMessageAsync(string phoneNumber,WhatsAppTemplate template);
    Task<string> UploadMediaAsync(Stream fileStream, string fileName);
}