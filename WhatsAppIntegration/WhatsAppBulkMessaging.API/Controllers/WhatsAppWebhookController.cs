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

    public WhatsAppWebhookController(IOptions<WhatsAppOptions> options,
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
            verifyToken == _options.VerifyToken)
        {
            return Ok(challenge);
        }

        return Unauthorized();
    }

    [HttpPost]
    public async Task<IActionResult> Receive(WhatsAppWebhookDto payload)
    {
        foreach (var entry in payload.Entry)
        {
            foreach (var change in entry.Changes)
            {
                if (change.Value is null)
                    continue;

                foreach (var message in change.Value.Messages)
                {
                    if (string.IsNullOrWhiteSpace(message.From) ||
                        string.IsNullOrWhiteSpace(message.Text?.Body))
                        continue;

                    var latestMessage =
                        await _messageRepository.GetLatestByPhoneNumberAsync(
                            message.From);

                    if (latestMessage is null)
                        continue;

                    DateTime replyReceivedAt = DateTime.UtcNow;

                    if (long.TryParse(
                        message.Timestamp,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var unixTimestamp))
                    {
                        replyReceivedAt = DateTimeOffset
                            .FromUnixTimeSeconds(unixTimestamp)
                            .UtcDateTime;
                    }

                    await _messageRepository.UpdateReplyAsync(latestMessage.Id,message.Text.Body,replyReceivedAt);
                }
            }
        }

        return Ok();
    }

}