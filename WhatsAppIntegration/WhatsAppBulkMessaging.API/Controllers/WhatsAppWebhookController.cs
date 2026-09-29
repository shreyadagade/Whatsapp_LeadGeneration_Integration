//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using System.Text.Json;
//using WhatsAppBulkMessaging.Application.Interfaces;

//namespace WhatsAppBulkMessaging.API.Controllers;

//[ApiController]
//[Route("api/whatsapp/webhook")]
//public class WhatsAppWebhookController : ControllerBase
//{
//    private readonly IConfiguration _configuration;
//    private readonly IWhatsAppMessageRepository _messageRepository;

//    public WhatsAppWebhookController(
//            IConfiguration configuration,
//            IWhatsAppMessageRepository messageRepository)
//    {
//        _configuration = configuration;
//        _messageRepository = messageRepository;
//    }

//    [HttpGet]
//    public IActionResult VerifyWebhook(
//        [FromQuery(Name = "hub.mode")] string mode,
//        [FromQuery(Name = "hub.verify_token")] string verifyToken,
//        [FromQuery(Name = "hub.challenge")] string challenge)
//    {
//        var configuredToken =
//            _configuration["WhatsAppWebhook:VerifyToken"];

//        if (mode == "subscribe" &&
//            verifyToken == configuredToken)
//        {
//            return Content(challenge);
//        }

//        return Unauthorized();
//    }

//    [HttpPost]
//    public async Task<IActionResult> ReceiveWebhook([FromBody] JsonElement payload)
//    {
//        //Console.WriteLine("===== WHATSAPP WEBHOOK RECEIVED =====");
//        //Console.WriteLine("===== RAW WEBHOOK PAYLOAD =====");
//        //Console.WriteLine(payload.ToString());

//        try
//        {
//            if (!payload.TryGetProperty("entry", out var entries))
//                return Ok();

//            foreach (var entry in entries.EnumerateArray())
//            {
//                if (!entry.TryGetProperty("changes", out var changes))
//                    continue;

//                foreach (var change in changes.EnumerateArray())
//                {
//                    if (!change.TryGetProperty("value", out var value))
//                        continue;

//                    if (!value.TryGetProperty("statuses", out var statuses))
//                        continue;

//                    foreach (var status in statuses.EnumerateArray())
//                    {
//                        var messageId =
//                            status.GetProperty("id").GetString();

//                        var messageStatus =
//                            status.GetProperty("status").GetString();

//                        Console.WriteLine(
//                            $"WhatsApp Message: {messageId}, Status: {messageStatus}");

//                        string? failureReason = null;

//                        if (messageStatus == "failed" &&
//                            status.TryGetProperty("errors", out var errors) &&
//                            errors.GetArrayLength() > 0)
//                        {
//                            var error = errors[0];

//                            var errorCode = error.TryGetProperty("code", out var code)
//                                ? code.ToString()
//                                : null;

//                            var errorTitle = error.TryGetProperty("title", out var title)
//                                ? title.GetString()
//                                : null;

//                            var errorMessage = error.TryGetProperty("message", out var message)
//                                ? message.GetString()
//                                : null;

//                            failureReason =
//                                $"Code: {errorCode}, Title: {errorTitle}, Message: {errorMessage}";
//                        }

//                        await _messageRepository.UpdateStatusAsync(
//                            messageId!,
//                            messageStatus!,
//                            failureReason);
//                    }
//                }
//            }

//            return Ok();
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine(
//                $"Webhook processing failed: {ex.Message}");

//            return Ok();
//        }
//    }
//}


using Microsoft.AspNetCore.Mvc;
using WhatsAppBulkMessaging.API.Models;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.API.Controllers;

[ApiController]
[Route("api/whatsapp/webhook")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly IWhatsAppMessageRepository _messageRepository;
    private readonly IConfiguration _configuration;

    public WhatsAppWebhookController(
        IWhatsAppMessageRepository messageRepository,
        IConfiguration configuration)
    {
        _messageRepository = messageRepository;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Verify(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        if (mode == "subscribe" && verifyToken == _configuration["WhatsApp:VerifyToken"])
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
                {
                    continue;
                }

                foreach (var status in change.Value.Statuses)
                {
                    if (string.IsNullOrWhiteSpace(status.Id) ||
                        string.IsNullOrWhiteSpace(status.Status))
                    {
                        continue;
                    }

                    await _messageRepository.UpdateStatusAsync(
                        status.Id,
                        status.Status);
                }
            }
        }

        return Ok();
    }
}