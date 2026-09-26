//using WhatsAppBulkMessaging.Domain.Entities;

//namespace WhatsAppBulkMessaging.Application.Interfaces;

//public interface IWhatsAppTemplateService
//{
//    Task<List<WhatsAppTemplate>> GetAllActiveTemplatesAsync();

//    Task<WhatsAppTemplate> AddTemplateAsync(
//        WhatsAppTemplate template);

//    Task<WhatsAppTemplate> UpdateTemplateAsync(
//        WhatsAppTemplate template);
//}

using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppTemplateService
{
    Task<List<WhatsAppTemplate>> GetTemplatesAsync();

    Task SyncTemplatesAsync();
}