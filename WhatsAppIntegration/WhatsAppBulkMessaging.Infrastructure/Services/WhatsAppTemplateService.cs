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
    private readonly IWhatsAppMetaService _metaService;

    public WhatsAppTemplateService(
            IWhatsAppTemplateRepository repository,
            IWhatsAppMetaService metaService)
    {
        _repository = repository;
        _metaService = metaService;
    }

    public async Task<List<WhatsAppTemplate>> GetTemplatesAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task SyncTemplatesAsync()
    {
        var metaResponse = await _metaService.GetTemplatesAsync();

        var approvedTemplates = metaResponse.Data
            .Where(x =>
                string.Equals(
                    x.Status,
                    "APPROVED",
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        var approvedTemplateKeys = approvedTemplates
            .Select(x => $"{x.Name}|{x.Language}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var metaTemplate in approvedTemplates)
        {
            var existingTemplate = await _repository.GetByNameAsync(
                metaTemplate.Name,
                metaTemplate.Language);

            if (existingTemplate is null)
            {
                var newTemplate = new WhatsAppTemplate
                {
                    TemplateName = metaTemplate.Name,
                    LanguageCode = metaTemplate.Language,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastSyncedAt = DateTime.UtcNow
                };

                await _repository.AddAsync(newTemplate);
            }
            else
            {
                existingTemplate.IsActive = true;
                existingTemplate.LastSyncedAt = DateTime.UtcNow;
                existingTemplate.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(existingTemplate);
            }
        }

        var localActiveTemplates = await _repository.GetAllAsync();

        foreach (var localTemplate in localActiveTemplates)
        {
            var key = $"{localTemplate.TemplateName}|{localTemplate.LanguageCode}";

            if (!approvedTemplateKeys.Contains(key))
            {
                localTemplate.IsActive = false;
                localTemplate.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(localTemplate);
            }
        }
    }
}