using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class RecipientValidationService : IRecipientValidationService
{
    private readonly IPhoneNumberService _phoneNumberService;

    public RecipientValidationService(
        IPhoneNumberService phoneNumberService)
    {
        _phoneNumberService = phoneNumberService;
    }

    public List<RecipientValidationResultDto> Validate(
        List<RecipientDto> recipients)
    {
        var results = new List<RecipientValidationResultDto>();

        var seenPhoneNumbers = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var recipient in recipients)
        {
            var result = new RecipientValidationResultDto
            {
                RowNumber = recipient.RowNumber,
                SerialNumber = recipient.SerialNumber,
                CandidateName = recipient.CandidateName,
                OriginalPhoneNumber = recipient.PhoneNumber
            };

            var normalizedNumber =
                _phoneNumberService.Normalize(
                    recipient.PhoneNumber);

            if (normalizedNumber is null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Invalid phone number.";

                results.Add(result);
                continue;
            }

            result.NormalizedPhoneNumber = normalizedNumber;

            if (!seenPhoneNumbers.Add(normalizedNumber))
            {
                result.IsValid = false;
                result.ErrorMessage = "Duplicate phone number.";

                results.Add(result);
                continue;
            }

            result.IsValid = true;

            results.Add(result);
        }

        return results;
    }
}