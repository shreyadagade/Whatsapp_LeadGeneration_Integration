
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WhatsAppBulkMessaging.API.Hubs;
using WhatsAppBulkMessaging.API.Models;
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Domain.Entities;
using WhatsAppBulkMessaging.Infrastructure.Repositories;

namespace WhatsAppBulkMessaging.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _messageService;
    private readonly IWhatsAppMessageRepository _messageRepository;
    private readonly IWhatsAppTemplateRepository _templateRepository;
    private readonly IExcelService _excelService;
    private readonly IRecipientValidationService _recipientValidationService;
    private readonly IWhatsAppMetaService _metaService;
    private readonly IWhatsAppMessageHistoryService _messageHistoryService;
    private readonly WhatsAppChatMessageRepository _chatRepository;
    private readonly IHubContext<WhatsAppChatHub> _hubContext;

    public WhatsAppController(
        IWhatsAppService messageService,
        IWhatsAppMessageRepository messageRepository,
        IWhatsAppTemplateRepository templateRepository,
        IExcelService excelService,
        IRecipientValidationService recipientValidationService,
        IWhatsAppMetaService metaService,
        IWhatsAppMessageHistoryService messageHistoryService,
        WhatsAppChatMessageRepository chatRepository,
        IHubContext<WhatsAppChatHub> hubContext)
    {
        _messageService = messageService;
        _messageRepository = messageRepository;
        _templateRepository = templateRepository;
        _excelService = excelService;
        _recipientValidationService = recipientValidationService;
        _metaService = metaService;
        _messageHistoryService = messageHistoryService;
        _chatRepository = chatRepository;
        _hubContext = hubContext;
    }

 
    [HttpPost("send-bulk")]
    public async Task<IActionResult> SendBulk(SendBulkMessageRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new
            {
                message = "Recipient file is required."
            });
        }

        var allowedExtensions = new[]
        {
        ".xlsx",
        ".xls",
        ".csv",
        ".tsv"
    };

        var extension = Path.GetExtension(request.File.FileName);

        if (!allowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Unsupported file format."
            });
        }

        using var stream = request.File.OpenReadStream();

        var template = await _templateRepository.GetByIdAsync(request.WhatsAppTemplateId);

        if (template is null)
        {
            return BadRequest(new
            {
                message = "Selected WhatsApp template was not found or is inactive."
            });
        }

        string? headerMediaId = null;

        if (request.HeaderImage is not null &&
            request.HeaderImage.Length > 0)
        {
            var allowedImageTypes = new[]
            {
            "image/jpeg",
            "image/png"
        };

            if (!allowedImageTypes.Contains(
                request.HeaderImage.ContentType,
                StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Only JPG and PNG images are supported."
                });
            }

            await using var imageStream =
                request.HeaderImage.OpenReadStream();

            headerMediaId = await _metaService.UploadMediaAsync(
                imageStream,
                request.HeaderImage.FileName,
                request.HeaderImage.ContentType);
        }

        var recipients =
            await _excelService.ReadRecipientsAsync(
                stream,
                request.File.FileName);

        var validationResults =
            _recipientValidationService.Validate(recipients);

        if (!validationResults.Any(x => x.IsValid))
        {
            return BadRequest(new
            {
                message = "No valid recipients were found.",
                totalRecipients = recipients.Count,
                invalidRecipients = validationResults.Count(x => !x.IsValid),
                recipients = validationResults
            });
        }

        var sentMessages = new System.Collections.Concurrent.ConcurrentBag<object>();

        var semaphore = new SemaphoreSlim(10);

        var templateId = template.Id;
        var templateName = template.TemplateName;
        var languageCode = template.LanguageCode;
        var bodyParameterCount = template.BodyParameterCount;
        var templateBodyText = template.BodyText;

        var tasks = validationResults
            .Where(x => x.IsValid)
            .Select(async recipient =>
            {
                await semaphore.WaitAsync();

                try
                {
                    var sendRequest = new MetaSendMessageRequestDto
                    {
                        To = recipient.NormalizedPhoneNumber!,
                        Type = "template",
                        Template = new MetaSendTemplateDto
                        {
                            Name = templateName,
                            Language = new MetaSendTemplateLanguageDto
                            {
                                Code = languageCode
                            },
                            Components = BuildTemplateComponents(
                                bodyParameterCount,
                                recipient.CandidateName,
                                headerMediaId)
                        }
                    };

                    try
                    {
                        var messageId = await _messageService.SendTemplateMessageAsync(sendRequest);

                        var message = new WhatsAppMessage
                        {
                            PhoneNumber = recipient.NormalizedPhoneNumber!,
                            CandidateName = recipient.CandidateName,
                            WhatsAppTemplateId = templateId,
                            Status = "Sent",
                            MetaMessageId = messageId,
                            CreatedAt = DateTime.UtcNow,
                            SentAt = DateTime.UtcNow
                        };

                        await _messageRepository.AddAsync(message);


                        var chatMessage = new WhatsAppChatMessage
                        {
                            PhoneNumber = recipient.NormalizedPhoneNumber!,
                            Direction = "Outgoing",
                            MessageType = headerMediaId != null ? "ImageTemplate" : "Template",
                            MessageText = template.BodyText ?? $"Template: {templateName}",
                            MediaId = headerMediaId,
                            MetaMessageId = messageId,
                            Status = "Sent",
                            CreatedAt = message.CreatedAt,
                            SentAt = message.SentAt
                        };

                        await _chatRepository.AddAsync(chatMessage);

                        await _hubContext.Clients.All.SendAsync("ReceiveMessage",
                        new
                        {
                            phoneNumber = chatMessage.PhoneNumber,
                            direction = chatMessage.Direction,
                            messageType = chatMessage.MessageType,
                            messageText = chatMessage.MessageText,
                            createdAt = chatMessage.CreatedAt
                        });

                        sentMessages.Add(new
                        {
                            rowNumber = recipient.RowNumber,
                            phoneNumber = recipient.NormalizedPhoneNumber,
                            status = "Sent",
                            messageId
                        });
                    }
                    catch (Exception ex)
                    {
                        var failedMessage = new WhatsAppMessage
                        {
                            PhoneNumber = recipient.NormalizedPhoneNumber!,
                            CandidateName = recipient.CandidateName,
                            WhatsAppTemplateId = templateId,
                            Status = "Failed",
                            ErrorMessage = ex.Message,
                            RetryCount = 0,
                            CreatedAt = DateTime.UtcNow,
                            FailedAt = DateTime.UtcNow
                        };

                        await _messageRepository.AddAsync(failedMessage);

                        sentMessages.Add(new
                        {
                            rowNumber = recipient.RowNumber,
                            phoneNumber = recipient.NormalizedPhoneNumber,
                            status = "Failed",
                            error = ex.Message
                        });
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

        await Task.WhenAll(tasks);

        semaphore.Dispose();

        return Ok(new
        {
            message = "Bulk message processing completed.",
            templateId = request.WhatsAppTemplateId,
            totalRecipients = recipients.Count,
            validRecipients = validationResults.Count(x => x.IsValid),
            invalidRecipients = validationResults.Count(x => !x.IsValid),
            recipients = validationResults,
            sentMessages = sentMessages.ToList()
        });
    }

    private static List<MetaSendTemplateComponentDto> BuildTemplateComponents(
            int bodyParameterCount,
            string? candidateName,
            string? headerMediaId)
    {
        var components = new List<MetaSendTemplateComponentDto>();

        if (!string.IsNullOrWhiteSpace(headerMediaId))
        {
            components.Add(
                new MetaSendTemplateComponentDto
                {
                    Type = "header",
                    Parameters =
                    [
                        new MetaSendTemplateParameterDto
                    {
                        Type = "image",
                        Image = new MetaSendTemplateImageDto
                        {
                            Id = headerMediaId
                        }
                    }
                    ]
                });
        }

        if (bodyParameterCount > 0 && !string.IsNullOrWhiteSpace(candidateName))
        {
            components.Add(
                new MetaSendTemplateComponentDto
                {
                    Type = "body",
                    Parameters =
                    [
                        new MetaSendTemplateParameterDto
                    {
                        Type = "text",
                        Text = candidateName
                    }
                    ]
                });
        }

        return components;
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetAllMessages()
    {
        var messages = await _messageHistoryService.GetAllMessagesAsync();

        return Ok(messages);
    }

   
    [HttpGet("messages/{phoneNumber}")]
    public async Task<IActionResult> GetMessagesByPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return BadRequest(new
            {
                message = "Phone number is required."
            });
        }

        var messages =
            await _messageHistoryService
                .GetMessagesByPhoneNumberAsync(phoneNumber);

        return Ok(messages);
    }

    [HttpPost("reply")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SendReply([FromForm] SendReplyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest(new
            {
                message = "Phone number is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.MessageType))
        {
            return BadRequest(new
            {
                message = "Message type is required."
            });
        }

        var messageType = request.MessageType.Trim().ToLowerInvariant();

        if (messageType is not ("text" or "image" or "video" or "audio" or "document"))
        {
            return BadRequest(new
            {
                message = "Supported types: text, image, video, audio, document."
            });
        }

        if (messageType == "text" && string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new
            {
                message = "Text is required for text messages."
            });
        }

        if (messageType != "text" && (request.File == null || request.File.Length == 0))
        {
            return BadRequest(new
            {
                message = "A file is required for media messages."
            });
        }

        await using var fileStream = request.File?.OpenReadStream();

        var replyRequest = new SendReplyRequestDto
        {
            PhoneNumber = request.PhoneNumber,
            MessageType = messageType,
            Text = request.Text,
            Caption = request.Caption,
            FileStream = fileStream,
            FileName = request.File?.FileName,
            ContentType = request.File?.ContentType
        };

        try
        {
            var result = await _metaService.SendReplyAsync(replyRequest);


            var sentAt = DateTime.UtcNow;

            // Save text message separately
            if (!string.IsNullOrWhiteSpace(result.TextMessageId))
            {
                var textMessage = new WhatsAppChatMessage
                {
                    PhoneNumber = request.PhoneNumber,
                    Direction = "Outgoing",
                    MessageType = "text",
                    MessageText = request.Text,
                    MetaMessageId = result.TextMessageId,
                    Status = "Sent",
                    CreatedAt = sentAt,
                    SentAt = sentAt
                };

                await _chatRepository.AddAsync(textMessage);
            }

            // Save media message separately
            if (!string.IsNullOrWhiteSpace(result.MediaMessageId))
            {
                var mediaMessage = new WhatsAppChatMessage
                {
                    PhoneNumber = request.PhoneNumber,
                    Direction = "Outgoing",
                    MessageType = messageType,
                    MessageText = request.Caption,
                    MediaId = result.MediaId,
                    MimeType = result.MimeType,
                    FileName = result.FileName,
                    MetaMessageId = result.MediaMessageId,
                    Status = "Sent",
                    CreatedAt = sentAt,
                    SentAt = sentAt
                };

                await _chatRepository.AddAsync(mediaMessage);
            }

            return Ok(new
            {
                message = result.Message,
                phoneNumber = result.PhoneNumber,
                messageType = result.MessageType,
                textMessageId = result.TextMessageId,
                mediaMessageId = result.MediaMessageId
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message = "WhatsApp reply could not be sent.",
                    error = ex.Message
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("chat/{phoneNumber}")]
    public async Task<IActionResult> GetChatHistoryByPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return BadRequest(new { message = "Phone number is required." });

        var messages =
            await _messageHistoryService.GetChatHistoryByPhoneNumberAsync(phoneNumber);

        return Ok(messages);
    }


    [HttpGet("chats")]
    public async Task<IActionResult> GetChatList()
    {
        var chats = await _messageHistoryService.GetChatListAsync();
        return Ok(chats);
    }

    [HttpPut("chats/{phoneNumber}/read")]
    public async Task<IActionResult> MarkChatAsRead(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return BadRequest(new { message = "Phone number is required." });

        await _messageHistoryService.MarkChatAsReadAsync(phoneNumber);

        return Ok(new { message = "Chat marked as read." });
    }

    [HttpGet("media/{mediaId}")]
    public async Task<IActionResult> GetMedia(string mediaId)
    {
        var media = await _metaService.DownloadMediaAsync(mediaId);

        var chatMessage = await _chatRepository.GetByMediaIdAsync(mediaId);

        var fileName = chatMessage?.FileName;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = mediaId;
        }

        return File(
            media.Content,
            media.ContentType,
            fileName);
    }


}