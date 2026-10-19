
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Domain.Entities;

namespace WhatsAppBulkMessaging.Infrastructure.Services;

public class WhatsAppTemplateService : IWhatsAppTemplateService
{
    private readonly IWhatsAppTemplateRepository _repository;
    private readonly IWhatsAppMetaService _metaService;

    public WhatsAppTemplateService(IWhatsAppTemplateRepository repository,
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

        var approvedTemplates = metaResponse.Data.Where(x =>string.Equals(
                    x.Status,
                    "APPROVED",
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        var approvedTemplateKeys = approvedTemplates
            .Select(x => $"{x.Name}|{x.Language}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var metaTemplate in approvedTemplates)
        {
            var bodyComponent = metaTemplate.Components
                .FirstOrDefault(x =>string.Equals(x.Type,"BODY",StringComparison.OrdinalIgnoreCase));

            var bodyParameterCount = 0;

            if (bodyComponent is not null &&
                !string.IsNullOrWhiteSpace(bodyComponent.Text))
            {
                bodyParameterCount = System.Text.RegularExpressions.Regex
                        .Matches(bodyComponent.Text, @"\{\{\d+\}\}").Count;
            }

            metaTemplate.BodyParameterCount = bodyParameterCount;

            // Get template body text
            var bodyText = bodyComponent?.Text;

            // Get template header format
            var headerComponent = metaTemplate.Components
                .FirstOrDefault(x =>
                    string.Equals(
                        x.Type,
                        "HEADER",
                        StringComparison.OrdinalIgnoreCase));

            var headerFormat = headerComponent?.Format;

            var headerImageUrl = headerComponent?.Example?.Header_Handle?
                .FirstOrDefault();

            var existingTemplate = await _repository.GetByNameAsync(
                metaTemplate.Name,
                metaTemplate.Language);

            if (existingTemplate is null)
            {
                var newTemplate = new WhatsAppTemplate
                {
                    TemplateName = metaTemplate.Name,
                    LanguageCode = metaTemplate.Language,
                    BodyParameterCount = metaTemplate.BodyParameterCount,
                    BodyText = bodyText,
                    HeaderFormat = headerFormat,
                    HeaderImageUrl = headerImageUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastSyncedAt = DateTime.UtcNow
                };

                await _repository.AddAsync(newTemplate);
            }
            else
            {
                existingTemplate.IsActive = true;

                existingTemplate.BodyParameterCount = metaTemplate.BodyParameterCount;

                existingTemplate.BodyText = bodyText;
                existingTemplate.HeaderFormat = headerFormat;
                existingTemplate.HeaderImageUrl = headerImageUrl;

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
