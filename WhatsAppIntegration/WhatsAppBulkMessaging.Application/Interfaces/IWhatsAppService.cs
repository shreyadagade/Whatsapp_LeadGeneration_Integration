namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppService
{
    Task<string> SendTemplateMessageAsync(
        string phoneNumber,
        string templateName,
        string? headerImageMediaId = null);
    Task<string> UploadMediaAsync(Stream fileStream, string fileName);
}