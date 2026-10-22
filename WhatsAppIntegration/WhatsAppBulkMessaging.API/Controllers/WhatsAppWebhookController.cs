
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Domain.Entities;
using WhatsAppBulkMessaging.Infrastructure.Repositories;

namespace WhatsAppBulkMessaging.API.Controllers;

[ApiController]
[Route("api/whatsapp/webhook")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly WhatsAppOptions _options;
    private readonly IWhatsAppMessageRepository _messageRepository;
    private readonly IWhatsAppTemplateService _templateService;
    private readonly WhatsAppChatMessageRepository _chatRepository;

    public WhatsAppWebhookController(
        IOptions<WhatsAppOptions> options,
        IWhatsAppMessageRepository messageRepository,
        IWhatsAppTemplateService templateService,
        WhatsAppChatMessageRepository chatRepository)
    {
        _options = options.Value;
        _messageRepository = messageRepository;
        _templateService = templateService;
        _chatRepository = chatRepository;
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
     
        if (payload.Entry is null || payload.Entry.Count == 0)
            return Ok();

        foreach (var entry in payload.Entry)
        {
            foreach (var change in entry.Changes)
            {
                // A. Automatically synchronize WhatsApp templates
                if (string.Equals(change.Field,"message_template_status_update",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await _templateService.SyncTemplatesAsync();
                    continue;
                }

                var value = change.Value;

                if (value is null)
                    continue;

                // B. Update outgoing message statuses
                foreach (var statusUpdate in value.Statuses)
                {
                    if (string.IsNullOrWhiteSpace(statusUpdate.Id) || string.IsNullOrWhiteSpace(statusUpdate.Status))
                    {
                        continue;
                    }

                    DateTime? statusTime = null;

                    if (long.TryParse(statusUpdate.Timestamp,NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var unixTimestamp))
                    {
                        statusTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
                    }

                    var error = statusUpdate.Errors.FirstOrDefault();

                    var errorMessage = error is null ? null: $"Code: {error.Code}; " +
                          $"{error.Title}; {error.Message}; " +
                          $"{error.ErrorData?.Details}";

                    await _messageRepository.UpdateStatusAsync(statusUpdate.Id,
                        statusUpdate.Status,statusTime,errorMessage);
                }

                // C. Save incoming messages in unified chat history
                foreach (var incoming in value.Messages)
                {
                    //Console.WriteLine($"INCOMING REPLY: From={incoming.From}, Id={incoming.Id}, Text={incoming.Text?.Body}");

                    if (string.IsNullOrWhiteSpace(incoming.From))
                        continue;

                    // Required to prevent duplicate webhook entries
                    if (string.IsNullOrWhiteSpace(incoming.Id))
                        continue;

                    string replyText;

                    if (!string.IsNullOrWhiteSpace(incoming.Text?.Body))
                    {
                        replyText = incoming.Text.Body;
                    }
                    else
                    {
                        replyText = $"[{incoming.Type ?? "unknown"} message received]";
                    }

                    DateTime replyReceivedAt = DateTime.UtcNow;

                    if (long.TryParse(incoming.Timestamp,NumberStyles.Integer,CultureInfo.InvariantCulture,
                        out var replyTimestamp))
                    {
                        replyReceivedAt = DateTimeOffset.FromUnixTimeSeconds(replyTimestamp)
                            .UtcDateTime;
                    }

                    // Save incoming message in unified chat history
                    if (!await _chatRepository.ExistsAsync(incoming.Id))
                    {
                        var chatMessage = new WhatsAppChatMessage
                        {
                            PhoneNumber = incoming.From,
                            Direction = "Incoming",
                            MessageType = incoming.Type ?? "text",
                            MessageText = replyText,
                            MetaMessageId = incoming.Id,
                            Status = "Received",
                            CreatedAt = replyReceivedAt
                        };

                        await _chatRepository.AddAsync(chatMessage);
                    }
                }
            }
        }

        return Ok();
    }
}
