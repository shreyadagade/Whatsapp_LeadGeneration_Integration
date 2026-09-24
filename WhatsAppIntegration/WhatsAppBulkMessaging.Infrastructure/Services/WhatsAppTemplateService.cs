//using WhatsAppBulkMessaging.Application.Interfaces;
//using WhatsAppBulkMessaging.Domain.Entities;

//namespace WhatsAppBulkMessaging.Application.Services;

//public class WhatsAppTemplateService : IWhatsAppTemplateService
//{
//    private readonly IWhatsAppTemplateRepository _templateRepository;

//    public WhatsAppTemplateService(IWhatsAppTemplateRepository templateRepository)
//    {
//        _templateRepository = templateRepository;
//    }

//    public async Task<List<WhatsAppTemplate>> GetAllActiveTemplatesAsync()
//    {
//        return await _templateRepository.GetAllActiveAsync();
//    }

//    public async Task<WhatsAppTemplate> AddTemplateAsync(WhatsAppTemplate template)
//    {
//        if (template == null)
//        {
//            throw new ArgumentNullException(nameof(template));
//        }

//        if (string.IsNullOrWhiteSpace(template.TemplateName))
//        {
//            throw new ArgumentException(
//                "Template name is required.");
//        }

//        if (string.IsNullOrWhiteSpace(template.LanguageCode))
//        {
//            throw new ArgumentException(
//                "Language code is required.");
//        }

//        if (!string.IsNullOrWhiteSpace(template.HeaderType) &&
//            template.HeaderType.Equals(
//                "IMAGE",
//                StringComparison.OrdinalIgnoreCase) &&
//            string.IsNullOrWhiteSpace(template.HeaderMediaId))
//        {
//            throw new ArgumentException(
//                "Header media ID is required for IMAGE header templates.");
//        }

//        var existingTemplate = await _templateRepository.GetByTemplateNameAsync(
//        template.TemplateName.Trim());

//        if (existingTemplate != null)
//        {
//            throw new ArgumentException(
//                $"WhatsApp template '{template.TemplateName}' already exists.");
//        }

//        await _templateRepository.AddAsync(template);

//        return template;
//    }

//    public async Task<WhatsAppTemplate> UpdateTemplateAsync(WhatsAppTemplate template)
//    {
//        if (template == null)
//        {
//            throw new ArgumentNullException(nameof(template));
//        }

//        await _templateRepository.UpdateAsync(template);

//        return template;
//    }
//}

using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class WhatsAppTemplateService : IWhatsAppTemplateService
{
    private readonly IWhatsAppTemplateRepository _repository;

    public WhatsAppTemplateService(
        IWhatsAppTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<WhatsAppTemplate>> GetTemplatesAsync()
    {
        return await _repository.GetAllAsync();
    }
}