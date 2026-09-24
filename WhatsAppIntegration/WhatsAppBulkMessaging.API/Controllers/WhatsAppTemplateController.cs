using Microsoft.AspNetCore.Mvc;
using WhatsAppBulkMessaging.Application.Interfaces;

namespace WhatsAppBulkMessaging.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppTemplatesController : ControllerBase
{
    private readonly IWhatsAppTemplateService _templateService;

    public WhatsAppTemplatesController(
        IWhatsAppTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _templateService.GetTemplatesAsync();

        return Ok(templates);
    }
}