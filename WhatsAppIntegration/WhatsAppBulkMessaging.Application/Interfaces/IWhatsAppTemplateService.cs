

using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IWhatsAppTemplateService
{
    Task<List<WhatsAppTemplate>> GetTemplatesAsync();

    Task SyncTemplatesAsync();
}