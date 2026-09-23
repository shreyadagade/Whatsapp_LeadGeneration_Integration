using WhatsAppBulkMessaging.Application.DTOs;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IExcelService
{
    Task<List<RecipientDto>> ReadRecipientsAsync(Stream fileStream,string fileName);
}