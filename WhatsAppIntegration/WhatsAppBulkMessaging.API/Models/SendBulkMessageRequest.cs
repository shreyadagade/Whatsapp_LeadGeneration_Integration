using Microsoft.AspNetCore.Http;

namespace WhatsAppBulkMessaging.API.Models;

public class SendBulkMessageRequest
{
    public IFormFile File { get; set; } = default!;

    public string TemplateName { get; set; } = string.Empty;
}