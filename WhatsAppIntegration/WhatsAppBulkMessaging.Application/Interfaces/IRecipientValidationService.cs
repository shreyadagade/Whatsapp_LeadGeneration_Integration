using WhatsAppBulkMessaging.Application.DTOs;

namespace WhatsAppBulkMessaging.Application.Interfaces;

public interface IRecipientValidationService
{
    List<RecipientValidationResultDto> Validate(List<RecipientDto> recipients);
}