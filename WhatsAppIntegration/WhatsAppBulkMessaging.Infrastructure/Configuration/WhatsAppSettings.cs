namespace WhatsAppBulkMessaging.Infrastructure.Configuration;

public class WhatsAppSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string PhoneNumberId { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "en";
}