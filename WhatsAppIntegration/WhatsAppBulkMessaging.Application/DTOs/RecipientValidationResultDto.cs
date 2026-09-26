namespace WhatsAppBulkMessaging.Application.DTOs;

public class RecipientValidationResultDto
{
    public int RowNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? CandidateName { get; set; }

    public string OriginalPhoneNumber { get; set; } = string.Empty;

    public string? NormalizedPhoneNumber { get; set; }

    public bool IsValid { get; set; }

    public string? ErrorMessage { get; set; }
}