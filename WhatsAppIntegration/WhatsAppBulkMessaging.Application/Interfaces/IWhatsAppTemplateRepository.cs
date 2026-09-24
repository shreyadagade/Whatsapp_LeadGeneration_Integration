//using WhatsAppBulkMessaging.Domain.Entities;

//namespace WhatsAppBulkMessaging.Application.Interfaces;

//public interface IWhatsAppTemplateRepository
//{
//    Task<WhatsAppTemplate?> GetByTemplateNameAsync(
//        string templateName);

//    Task<List<WhatsAppTemplate>> GetAllActiveAsync();

//    Task AddAsync(WhatsAppTemplate template);

//    Task UpdateAsync(WhatsAppTemplate template);
//}


using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppTemplateRepository
{
    Task<List<WhatsAppTemplate>> GetAllAsync();
    Task<WhatsAppTemplate?> GetByNameAsync(string templateName,string languageCode);
    Task AddAsync(WhatsAppTemplate template);
}