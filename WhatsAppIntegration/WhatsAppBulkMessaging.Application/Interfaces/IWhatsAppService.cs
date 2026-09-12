namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppService
{
    Task<string> SendTemplateMessageAsync(string phoneNumber,string templateName);
    Task<string> UploadMediaAsync(Stream fileStream, string fileName);
}