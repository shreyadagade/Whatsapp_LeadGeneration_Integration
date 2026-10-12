
using Microsoft.AspNetCore.Mvc;
using WhatsAppBulkMessaging.API.Models;
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Domain.Entities;

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
    public WhatsAppController(
        IWhatsAppService messageService,
        IWhatsAppMessageRepository messageRepository,
        IWhatsAppTemplateRepository templateRepository,
        IExcelService excelService,
        IRecipientValidationService recipientValidationService,
        IWhatsAppMetaService metaService,
        IWhatsAppMessageHistoryService messageHistoryService)
    {
        _messageService = messageService;
        _messageRepository = messageRepository;
        _templateRepository = templateRepository;
        _excelService = excelService;
        _recipientValidationService = recipientValidationService;
        _metaService = metaService;
        _messageHistoryService = messageHistoryService;
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

        var template = await _templateRepository.GetByIdAsync(
            request.WhatsAppTemplateId);

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
                        var messageId =
                            await _messageService.SendTemplateMessageAsync(
                                sendRequest);

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

//    [HttpPost("upload-media")]
//    public async Task<IActionResult> UploadMedia(IFormFile file)
//    {
//        if (file == null || file.Length == 0)
//        {
//            return BadRequest(new
//            {
//                message = "Image file is required."
//            });
//        }

//        var allowedContentTypes = new[]
//        {
//        "image/jpeg",
//        "image/png"
//    };

//        if (!allowedContentTypes.Contains(
//            file.ContentType,
//            StringComparer.OrdinalIgnoreCase))
//        {
//            return BadRequest(new
//            {
//                message = "Only JPG and PNG images are supported."
//            });
//        }

//        await using var stream = file.OpenReadStream();

//        var mediaId = await _metaService.UploadMediaAsync(
//            stream,
//            file.FileName,
//            file.ContentType);

//        return Ok(new
//        {
//            message = "Media uploaded successfully.",
//            mediaId
//        });
//    }
}