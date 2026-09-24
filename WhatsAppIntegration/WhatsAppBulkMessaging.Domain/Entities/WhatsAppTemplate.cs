//namespace WhatsAppBulkMessaging.Domain.Entities;

//public class WhatsAppTemplate
//{
//    public int Id { get; set; }

//    public string TemplateName { get; set; } = string.Empty;

//    public string LanguageCode { get; set; } = "en";

//    public string? HeaderType { get; set; }

//    public string? HeaderMediaId { get; set; }

//    public bool IsActive { get; set; } = true;
//}

namespace WhatsAppBulkMessaging.Domain.Entities;

public class WhatsAppTemplate
{
    public int Id { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public string LanguageCode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }
}