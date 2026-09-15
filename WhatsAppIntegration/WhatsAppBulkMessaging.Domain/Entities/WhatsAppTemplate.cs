namespace WhatsAppBulkMessaging.Domain.Entities;

public class WhatsAppTemplate
{
    public int Id { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public string LanguageCode { get; set; } = "en";

    public string? HeaderType { get; set; }

    public string? HeaderMediaId { get; set; }

    public bool IsActive { get; set; } = true;
}