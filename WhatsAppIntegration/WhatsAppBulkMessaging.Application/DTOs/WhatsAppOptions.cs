namespace WhatsAppBulkMessaging.Application.DTOs;

public class WhatsAppOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = string.Empty;

    public string PhoneNumberId { get; set; } = string.Empty;

    public string BusinessAccountId { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;
}