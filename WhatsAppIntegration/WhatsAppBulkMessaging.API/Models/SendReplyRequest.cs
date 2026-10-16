
using Microsoft.AspNetCore.Http;

namespace WhatsAppBulkMessaging.API.Models;

public class SendReplyRequest
{
    public string PhoneNumber { get; set; } = string.Empty;

    // Supported values: text, image, video, audio, document
    public string MessageType { get; set; } = string.Empty;

    // Required for text messages
    public string? Text { get; set; }

    // Optional caption for image, video or document
    public string? Caption { get; set; }

    // Required for image, video, audio and document
    public IFormFile? File { get; set; }
}
