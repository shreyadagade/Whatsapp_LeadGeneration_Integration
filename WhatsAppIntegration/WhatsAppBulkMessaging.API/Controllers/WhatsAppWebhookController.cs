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

    public WhatsAppWebhookController(
        IOptions<WhatsAppOptions> options,
        IWhatsAppMessageRepository messageRepository)
    {
        _options = options.Value;
        _messageRepository = messageRepository;
    }

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

    [HttpPost]
    public async Task<IActionResult> Receive(WhatsAppWebhookDto payload)
    {
        if (payload.Entry is null)
            return Ok();

        foreach (var entry in payload.Entry)
        {
            foreach (var change in entry.Changes)
            {
                var value = change.Value;

                if (value is null)
                    continue;

                // 1. Outgoing message status notifications
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
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture,
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

                // 2. Incoming customer replies
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
                        // Preserve the type of non-text messages.
                        replyText = $"[{incoming.Type ?? "unknown"} message received]";
                    }

                    WhatsAppBulkMessaging.Domain.Entities.WhatsAppMessage?
                        outgoingMessage = null;

                    // Prefer the exact message the customer replied to.
                    if (!string.IsNullOrWhiteSpace(incoming.Context?.Id))
                    {
                        outgoingMessage =
                            await _messageRepository.GetByMetaMessageIdAsync(
                                incoming.Context.Id);
                    }

                    // Fallback for replies that have no context.id.
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
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture,
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