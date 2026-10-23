
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Options;
    using System.Globalization;
    using WhatsAppBulkMessaging.API.Hubs;
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
        private readonly IHubContext<WhatsAppChatHub> _hubContext;

        public WhatsAppWebhookController(
            IOptions<WhatsAppOptions> options,
            IWhatsAppMessageRepository messageRepository,
            IWhatsAppTemplateService templateService,
            WhatsAppChatMessageRepository chatRepository,
            IHubContext<WhatsAppChatHub> hubContext)
        {
            _options = options.Value;
            _messageRepository = messageRepository;
            _templateService = templateService;
            _chatRepository = chatRepository;
            _hubContext = hubContext;
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


                    await _messageRepository.UpdateStatusAsync(
                        statusUpdate.Id,
                        statusUpdate.Status,
                        statusTime,
                        errorMessage);

                    // Update status and timestamps in chat history
                    await _chatRepository.UpdateStatusByMetaMessageIdAsync(
                        statusUpdate.Id,
                        statusUpdate.Status,
                        statusTime,
                        errorMessage);

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

                        string? mediaId = null;
                        string? mimeType = null;
                        string? fileName = null;
                        string? caption = null;

                        switch (incoming.Type?.ToLowerInvariant())
                        {
                            case "image":
                                mediaId = incoming.Image?.Id;
                                mimeType = incoming.Image?.MimeType;
                                caption = incoming.Image?.Caption;
                                break;

                            case "video":
                                mediaId = incoming.Video?.Id;
                                mimeType = incoming.Video?.MimeType;
                                caption = incoming.Video?.Caption;
                                break;

                            case "audio":
                                mediaId = incoming.Audio?.Id;
                                mimeType = incoming.Audio?.MimeType;
                                fileName = incoming.Audio?.FileName;
                                break;

                            case "document":
                                mediaId = incoming.Document?.Id;
                                mimeType = incoming.Document?.MimeType;
                                fileName = incoming.Document?.FileName;
                                caption = incoming.Document?.Caption;
                                break;
                        }

                        string replyText = incoming.Text?.Body
                            ?? caption
                            ?? $"[{incoming.Type ?? "unknown"} message received]";

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
                                MediaId = mediaId,
                                MimeType = mimeType,
                                FileName = fileName,
                                Status = "Received",
                                IsRead = false,
                                CreatedAt = replyReceivedAt
                            };

                            await _chatRepository.AddAsync(chatMessage);

                        await _hubContext.Clients.All.SendAsync("ReceiveMessage",
                        new
                        {
                            id = chatMessage.Id,
                            phoneNumber = chatMessage.PhoneNumber,
                            direction = chatMessage.Direction,
                            messageType = chatMessage.MessageType,
                            messageText = chatMessage.MessageText,
                            mediaId = chatMessage.MediaId,
                            mimeType = chatMessage.MimeType,
                            fileName = chatMessage.FileName,
                            metaMessageId = chatMessage.MetaMessageId,
                            status = chatMessage.Status,
                            createdAt = chatMessage.CreatedAt
                        });
                    }
                    }
                }
            }

            return Ok();
        }
    }
