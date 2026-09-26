using Microsoft.AspNetCore.Mvc;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppTemplatesController : ControllerBase
{
    private readonly IWhatsAppTemplateService _templateService;
    private readonly IWhatsAppMetaService _metaService;

    public WhatsAppTemplatesController(
            IWhatsAppTemplateService templateService,
            IWhatsAppMetaService metaService)
    {
        _templateService = templateService;
        _metaService = metaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _templateService.GetTemplatesAsync();

        return Ok(templates);
    }

    [HttpGet("meta")]
    public async Task<IActionResult> GetMetaTemplates()
    {
        var templates = await _metaService.GetTemplatesAsync();

        return Ok(templates);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncTemplates()
    {
        await _templateService.SyncTemplatesAsync();

        return Ok(new
        {
            message = "WhatsApp templates synchronized successfully."
        });
    }
}