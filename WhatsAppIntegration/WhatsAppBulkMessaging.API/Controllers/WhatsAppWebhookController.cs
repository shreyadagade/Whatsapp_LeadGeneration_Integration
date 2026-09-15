using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.API.Controllers;

[ApiController]
[Route("api/whatsapp/webhook")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWhatsAppMessageRepository _messageRepository;

    public WhatsAppWebhookController(
            IConfiguration configuration,
            IWhatsAppMessageRepository messageRepository)
    {
        _configuration = configuration;
        _messageRepository = messageRepository;
    }

    [HttpGet]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string verifyToken,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        var configuredToken =
            _configuration["WhatsAppWebhook:VerifyToken"];

        if (mode == "subscribe" &&
            verifyToken == configuredToken)
        {
            return Content(challenge);
        }

        return Unauthorized();
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook([FromBody] JsonElement payload)
    {
        Console.WriteLine("===== WHATSAPP WEBHOOK RECEIVED =====");

        try
        {
            if (!payload.TryGetProperty("entry", out var entries))
                return Ok();

            foreach (var entry in entries.EnumerateArray())
            {
                if (!entry.TryGetProperty("changes", out var changes))
                    continue;

                foreach (var change in changes.EnumerateArray())
                {
                    if (!change.TryGetProperty("value", out var value))
                        continue;

                    if (!value.TryGetProperty("statuses", out var statuses))
                        continue;

                    foreach (var status in statuses.EnumerateArray())
                    {
                        var messageId =
                            status.GetProperty("id").GetString();

                        var messageStatus =
                            status.GetProperty("status").GetString();

                        Console.WriteLine(
                            $"WhatsApp Message: {messageId}, Status: {messageStatus}");

                        await _messageRepository.UpdateStatusAsync(
                            messageId!,
                            messageStatus!);
                    }
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Webhook processing failed: {ex.Message}");

            return Ok();
        }
    }
}