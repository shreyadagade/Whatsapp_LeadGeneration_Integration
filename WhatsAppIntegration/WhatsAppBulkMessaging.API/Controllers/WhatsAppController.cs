////using Microsoft.AspNetCore.Mvc;
////using WhatsAppBulkMessaging.API.Models;
////using WhatsAppBulkMessaging.Application.Interfaces;
////using WhatsAppBulkMessaging.Application.Validators;
////using WhatsAppBulkMessaging.Domain.Entities;

////namespace WhatsAppBulkMessaging.API.Controllers;

////[ApiController]
////[Route("api/[controller]")]
////public class WhatsAppController : ControllerBase
////{
////    private readonly IExcelService _excelService;
////    private readonly IWhatsAppService _whatsAppService;
////    private readonly ILogger<WhatsAppController> _logger;
////    private readonly IWhatsAppTemplateRepository _templateRepository;
////    private readonly IWhatsAppMessageRepository _messageRepository;

////    public WhatsAppController(
////        IExcelService excelService,
////        IWhatsAppService whatsAppService,
////        ILogger<WhatsAppController> logger,
////        IWhatsAppTemplateRepository templateRepository,
////        IWhatsAppMessageRepository messageRepository)
////    {
////        _excelService = excelService;
////        _whatsAppService = whatsAppService;
////        _logger = logger;
////        _templateRepository = templateRepository;
////        _messageRepository = messageRepository;
////    }

////    [HttpPost("send-bulk")]
////    [Consumes("multipart/form-data")]
////    public async Task<IActionResult> SendBulk([FromForm] SendBulkMessageRequest request)
////    {
////        if (request.File == null || request.File.Length == 0)
////        {
////            return BadRequest(new
////            {
////                StatusCode = StatusCodes.Status400BadRequest,
////                Message = "Excel file is required."
////            });
////        }

////        var allowedExtensions = new[]
////        {
////            ".xlsx",
////            ".xls",
////            ".csv",
////            ".tsv"
////        };

////        var fileExtension = Path.GetExtension(request.File.FileName);

////        if (!allowedExtensions.Contains(fileExtension,StringComparer.OrdinalIgnoreCase))
////        {
////            return BadRequest(new
////            {
////                StatusCode = StatusCodes.Status400BadRequest,
////                Message = "Only .xlsx, .xls, .csv and .tsv files are allowed."
////            });
////        }

////        if (string.IsNullOrWhiteSpace(request.TemplateName))
////        {
////            return BadRequest(new
////            {
////                StatusCode = StatusCodes.Status400BadRequest,
////                Message = "Template name is required."
////            });
////        }

////        var template = await _templateRepository.GetByTemplateNameAsync(request.TemplateName.Trim());

////        if (template == null)
////        {
////            return BadRequest(new
////            {
////                StatusCode = StatusCodes.Status400BadRequest,
////                Message =
////                    $"WhatsApp template '{request.TemplateName}' was not found or is inactive."
////            });
////        }

////        using var stream = request.File.OpenReadStream();

////        var recipients = await _excelService.ReadRecipientsAsync(stream,request.File.FileName);

////        recipients = recipients.GroupBy(x => x.PhoneNumber.Trim())
////            .Select(x => x.First())
////            .ToList();

////        if (recipients.Count > 500)
////        {
////            return BadRequest(new
////            {
////                StatusCode = StatusCodes.Status400BadRequest,
////                Message =
////                    "Maximum 500 unique recipients are allowed per Excel file."
////            });
////        }

////        if (recipients.Count == 0)
////        {
////            return BadRequest(new
////            {
////                StatusCode = StatusCodes.Status400BadRequest,
////                Message = "No recipients found in the Excel file."
////            });
////        }

////        var results = new List<object>();

////        foreach (var recipient in recipients)
////        {
////            if (!PhoneNumberValidator.IsValidIndianMobileNumber(
////                    recipient.PhoneNumber))
////            {
////                results.Add(new
////                {
////                    PhoneNumber = recipient.PhoneNumber,
////                    Status = "Failed",
////                    MessageId = (string?)null,
////                    Reason = "Invalid Indian mobile number."
////                });

////                continue;
////            }

////            try
////            {
////                var messageId = await _whatsAppService.SendTemplateMessageAsync(
////                            recipient.PhoneNumber,
////                            template);

////                await _messageRepository.AddAsync(
////                    new WhatsAppMessage
////                    {
////                        PhoneNumber = recipient.PhoneNumber,
////                        TemplateName = template.TemplateName,
////                        MetaMessageId = messageId,
////                        Status = "Accepted",
////                        CreatedAt = DateTime.UtcNow
////                    });

////                results.Add(new
////                {
////                    PhoneNumber = recipient.PhoneNumber,
////                    Status = "Accepted",
////                    MessageId = messageId
////                });
////            }
////            catch (Exception ex)
////            {
////                await _messageRepository.AddAsync(
////                    new WhatsAppMessage
////                    {
////                        PhoneNumber = recipient.PhoneNumber,
////                        TemplateName = template.TemplateName,
////                        MetaMessageId = null,
////                        Status = "Failed",
////                        FailureReason =
////                            "Failed to send WhatsApp message.",
////                        CreatedAt = DateTime.UtcNow
////                    });

////                _logger.LogError(
////                    ex,
////                    "Failed to send WhatsApp message to {PhoneNumber} using template {TemplateName}",
////                    recipient.PhoneNumber,
////                    request.TemplateName);

////                results.Add(new
////                {
////                    PhoneNumber = recipient.PhoneNumber,
////                    Status = "Failed",
////                    MessageId = (string?)null,
////                    Reason =
////                        "Failed to send WhatsApp message. Please try again later."
////                });
////            }
////        }

////        return Ok(new
////        {
////            StatusCode = StatusCodes.Status200OK,
////            Message = "Bulk WhatsApp message processing completed.",
////            TemplateName = request.TemplateName,
////            TotalRecipients = recipients.Count,
////            Results = results
////        });
////    }

////    [HttpPost("upload-media")]
////    [Consumes("multipart/form-data")]
////    public async Task<IActionResult> UploadMedia(int templateId,IFormFile file)
////    {
////        if (file == null || file.Length == 0)
////        {
////            return BadRequest(new
////            {
////                StatusCode = StatusCodes.Status400BadRequest,
////                Message = "Image file is required."
////            });
////        }

////        var extension = Path.GetExtension(file.FileName);

////        if (!extension.Equals(
////                ".jpg",
////                StringComparison.OrdinalIgnoreCase) &&
////            !extension.Equals(
////                ".jpeg",
////                StringComparison.OrdinalIgnoreCase) &&
////            !extension.Equals(
////                ".png",
////                StringComparison.OrdinalIgnoreCase))
////        {
////            return BadRequest(new
////            {
////                StatusCode = StatusCodes.Status400BadRequest,
////                Message = "Only JPG, JPEG and PNG images are allowed."
////            });
////        }

////        using var stream = file.OpenReadStream();

////        var mediaId =
////            await _whatsAppService.UploadMediaAsync(
////                stream,
////                file.FileName);

////        return Ok(new
////        {
////            StatusCode = StatusCodes.Status200OK,
////            Message = "Image uploaded successfully.",
////            MediaId = mediaId
////        });
////    }
////}

//using Microsoft.AspNetCore.Mvc;
//using WhatsAppBulkMessaging.Application.DTOs;
//using WhatsAppBulkMessaging.Application.Interfaces;
//using WhatsAppBulkMessaging.Domain.Entities;

//namespace WhatsAppBulkMessaging.API.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class WhatsAppController : ControllerBase
//{
//    private readonly IWhatsAppService _messageService;
//    private readonly IWhatsAppMessageRepository _messageRepository;

//    public WhatsAppController(IWhatsAppService messageService,
//          IWhatsAppMessageRepository messageRepository)
//    {
//        _messageService = messageService;
//        _messageRepository = messageRepository;
//    }

//    [HttpPost("send")]
//    public async Task<IActionResult> Send(MetaSendMessageRequestDto request)
//    {
//        var messageId =
//            await _messageService.SendTemplateMessageAsync(request);

//        var message = new WhatsAppMessage
//        {
//            PhoneNumber = request.To,
//            Status = "Sent",
//            MetaMessageId = messageId,
//            CreatedAt = DateTime.UtcNow,
//            SentAt = DateTime.UtcNow
//        };

//        await _messageRepository.AddAsync(message);

//        return Ok(new
//        {
//            messageId
//        });
//    }
//}


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

    public WhatsAppController(
        IWhatsAppService messageService,
        IWhatsAppMessageRepository messageRepository,
        IWhatsAppTemplateRepository templateRepository,
        IExcelService excelService,
        IRecipientValidationService recipientValidationService)
    {
        _messageService = messageService;
        _messageRepository = messageRepository;
        _templateRepository = templateRepository;
        _excelService = excelService;
        _recipientValidationService = recipientValidationService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(MetaSendMessageRequestDto request)
    {
        var template = await _templateRepository.GetByNameAsync(
            request.Template.Name,
            request.Template.Language.Code);

        if (template is null)
        {
            return BadRequest(new
            {
                message = "The selected WhatsApp template was not found or is inactive."
            });
        }

        var messageId =
            await _messageService.SendTemplateMessageAsync(request);

        var message = new WhatsAppMessage
        {
            PhoneNumber = request.To,
            WhatsAppTemplateId = template.Id,
            Status = "Sent",
            MetaMessageId = messageId,
            CreatedAt = DateTime.UtcNow,
            SentAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message);

        return Ok(new
        {
            messageId
        });
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

        var extension = Path.GetExtension(
            request.File.FileName);


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

        var recipients =
            await _excelService.ReadRecipientsAsync(
                stream,
                request.File.FileName);

        var validationResults =
            _recipientValidationService.Validate(recipients);

        var sentMessages = new List<object>();

        foreach (var recipient in validationResults.Where(x => x.IsValid))
        {
            var sendRequest = new MetaSendMessageRequestDto
            {
                To = recipient.NormalizedPhoneNumber!,
                Type = "template",
                Template = new MetaSendTemplateDto
                {
                    Name = template.TemplateName,
                    Language = new MetaSendTemplateLanguageDto
                    {
                        Code = template.LanguageCode
                    },
                    Components = BuildTemplateComponents(
                        recipient.CandidateName)
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
                    WhatsAppTemplateId = template.Id,
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
                    WhatsAppTemplateId = template.Id,
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

        return Ok(new
        {
            message = "Bulk message processing completed.",
            templateId = request.WhatsAppTemplateId,
            totalRecipients = recipients.Count,
            validRecipients = validationResults.Count(x => x.IsValid),
            invalidRecipients = validationResults.Count(x => !x.IsValid),
            recipients = validationResults,
            sentMessages
        });
    }

    private static List<MetaSendTemplateComponentDto> BuildTemplateComponents(string? candidateName)
    {
        if (string.IsNullOrWhiteSpace(candidateName))
        {
            return [];
        }

        return
        [
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
        }
        ];
    }
}