using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.API.Controllers;

[ApiController]
[Route("api/whatsapp/webhook")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly WhatsAppOptions _options;
    private readonly IWhatsAppMessageRepository _messageRepository;
    private readonly IWhatsAppTemplateService _templateService;

    public WhatsAppWebhookController(
        IOptions<WhatsAppOptions> options,
        IWhatsAppMessageRepository messageRepository,
        IWhatsAppTemplateService templateService)
    {
        _options = options.Value;
        _messageRepository = messageRepository;
        _templateService = templateService;
    }

    // 1. Meta webhook verification
    [HttpGet]
    public IActionResult Verify(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        if (mode == "subscribe" &&
            !string.IsNullOrWhiteSpace(_options.VerifyToken) &&
            verifyToken == _options.VerifyToken &&
            !string.IsNullOrWhiteSpace(challenge))
        {
            return Content(challenge, "text/plain");
        }

        return Unauthorized();
    }

    // 2. Receive Meta webhook notifications
    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromBody] WhatsAppWebhookDto payload)
    {
        if (payload.Entry is null || payload.Entry.Count == 0)
            return Ok();

        foreach (var entry in payload.Entry)
        {
            foreach (var change in entry.Changes)
            {
                // A. Automatically synchronize WhatsApp templates
                // when Meta sends a template status update event.
                if (string.Equals(
                    change.Field,
                    "message_template_status_update",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await _templateService.SyncTemplatesAsync();
                    continue;
                }

                var value = change.Value;

                if (value is null)
                    continue;

                // B. Update outgoing message statuses:
                // sent, delivered, read, failed
                foreach (var statusUpdate in value.Statuses)
                {
                    if (string.IsNullOrWhiteSpace(statusUpdate.Id) ||
                        string.IsNullOrWhiteSpace(statusUpdate.Status))
                    {
                        continue;
                    }

                    DateTime? statusTime = null;

                    if (long.TryParse(
                        statusUpdate.Timestamp,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var unixTimestamp))
                    {
                        statusTime = DateTimeOffset
                            .FromUnixTimeSeconds(unixTimestamp)
                            .UtcDateTime;
                    }

                    var error = statusUpdate.Errors.FirstOrDefault();

                    var errorMessage = error is null
                        ? null
                        : $"Code: {error.Code}; " +
                          $"{error.Title}; {error.Message}; " +
                          $"{error.ErrorData?.Details}";

                    await _messageRepository.UpdateStatusAsync(
                        statusUpdate.Id,
                        statusUpdate.Status,
                        statusTime,
                        errorMessage);
                }

                // C. Save incoming customer replies
                foreach (var incoming in value.Messages)
                {
                    if (string.IsNullOrWhiteSpace(incoming.From))
                        continue;

                    string replyText;

                    if (!string.IsNullOrWhiteSpace(incoming.Text?.Body))
                    {
                        replyText = incoming.Text.Body;
                    }
                    else
                    {
                        replyText =
                            $"[{incoming.Type ?? "unknown"} message received]";
                    }

                    WhatsAppBulkMessaging.Domain.Entities.WhatsAppMessage?
                        outgoingMessage = null;

                    // First, find the exact outgoing message
                    // to which the customer replied.
                    if (!string.IsNullOrWhiteSpace(incoming.Context?.Id))
                    {
                        outgoingMessage =
                            await _messageRepository.GetByMetaMessageIdAsync(
                                incoming.Context.Id);
                    }

                    // If context.id is unavailable, use the latest
                    // outgoing message for this phone number.
                    if (outgoingMessage is null)
                    {
                        outgoingMessage =
                            await _messageRepository.GetLatestByPhoneNumberAsync(
                                incoming.From);
                    }

                    if (outgoingMessage is null)
                        continue;

                    DateTime replyReceivedAt = DateTime.UtcNow;

                    if (long.TryParse(
                        incoming.Timestamp,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var replyTimestamp))
                    {
                        replyReceivedAt = DateTimeOffset
                            .FromUnixTimeSeconds(replyTimestamp)
                            .UtcDateTime;
                    }

                    await _messageRepository.UpdateReplyAsync(
                        outgoingMessage.Id,
                        replyText,
                        replyReceivedAt);
                }
            }
        }

        return Ok();
    }
}
