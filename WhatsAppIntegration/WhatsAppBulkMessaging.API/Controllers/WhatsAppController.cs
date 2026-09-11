using Microsoft.AspNetCore.Mvc;
using WhatsAppBulkMessaging.API.Models;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Application.Validators;

namespace WhatsAppBulkMessaging.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
    private readonly IExcelService _excelService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(IExcelService excelService,
        IWhatsAppService whatsAppService,
        ILogger<WhatsAppController> logger)
    {
        _excelService = excelService;
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    [HttpPost("send-bulk")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SendBulk(
    [FromForm] SendBulkMessageRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Excel file is required."
            });
        }

        if (!Path.GetExtension(request.File.FileName)
            .Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Only .xlsx Excel files are allowed."
            });
        }

        if (string.IsNullOrWhiteSpace(request.TemplateName))
        {
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Template name is required."
            });
        }

        using var stream = request.File.OpenReadStream();

        var recipients =
            await _excelService.ReadRecipientsAsync(stream);

        recipients = recipients
            .GroupBy(x => x.PhoneNumber.Trim())
            .Select(x => x.First())
            .ToList();

        if (recipients.Count == 0)
        {
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "No recipients found in the Excel file."
            });
        }

        var results = new List<object>();

        foreach (var recipient in recipients)
        {
            if (!PhoneNumberValidator.IsValidIndianMobileNumber(
                    recipient.PhoneNumber))
            {
                results.Add(new
                {
                    PhoneNumber = recipient.PhoneNumber,
                    Status = "Failed",
                    MessageId = (string?)null,
                    Reason = "Invalid Indian mobile number."
                });

                continue;
            }

            try
            {
                var messageId =
                    await _whatsAppService.SendTemplateMessageAsync(
                        recipient.PhoneNumber,
                        request.TemplateName);

                results.Add(new
                {
                    PhoneNumber = recipient.PhoneNumber,
                    Status = "Accepted",
                    MessageId = messageId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send WhatsApp message to {PhoneNumber} using template {TemplateName}",
                    recipient.PhoneNumber,
                    request.TemplateName);

                results.Add(new
                {
                    PhoneNumber = recipient.PhoneNumber,
                    Status = "Failed",
                    MessageId = (string?)null,
                    Reason = "Message could not be sent."
                });
            }
        }

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Bulk WhatsApp message processing completed.",
            TemplateName = request.TemplateName,
            TotalRecipients = recipients.Count,
            Results = results
        });
    }
}