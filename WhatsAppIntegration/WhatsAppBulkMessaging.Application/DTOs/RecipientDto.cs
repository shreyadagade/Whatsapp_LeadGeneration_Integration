

namespace WhatsAppBulkMessaging.Application.DTOs;

public class RecipientDto
{
    public int RowNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? CandidateName { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
}