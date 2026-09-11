namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppService
{
    Task<string> SendTemplateMessageAsync(string phoneNumber,string templateName);
}