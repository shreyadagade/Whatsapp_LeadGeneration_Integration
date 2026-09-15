using Microsoft.AspNetCore.Mvc;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppTemplateController : ControllerBase
{
    private readonly IWhatsAppTemplateService _templateService;

    public WhatsAppTemplateController(
        IWhatsAppTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTemplates()
    {
        var templates =
            await _templateService.GetAllActiveTemplatesAsync();

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            message = "WhatsApp templates retrieved successfully.",
            data = templates
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddTemplate([FromBody] WhatsAppTemplate template)
    {
        var result =
            await _templateService.AddTemplateAsync(template);

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            message = "WhatsApp template added successfully.",
            data = result
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTemplate([FromBody] WhatsAppTemplate template)
    {
        var result =
            await _templateService.UpdateTemplateAsync(template);

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            message = "WhatsApp template updated successfully.",
            data = result
        });
    }
}