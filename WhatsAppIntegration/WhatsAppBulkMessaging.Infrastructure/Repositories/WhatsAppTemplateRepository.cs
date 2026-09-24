//using Microsoft.EntityFrameworkCore;
//using WhatsAppBulkMessaging.Application.Interfaces;
//using WhatsAppBulkMessaging.Domain.Entities;
//using WhatsAppBulkMessaging.Infrastructure.Data;

//namespace WhatsAppBulkMessaging.Infrastructure.Repositories;

//public class WhatsAppTemplateRepository
//    : IWhatsAppTemplateRepository
//{
//    private readonly AppDbContext _context;

//    public WhatsAppTemplateRepository(AppDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<WhatsAppTemplate?> GetByTemplateNameAsync(
//        string templateName)
//    {
//        return await _context.WhatsAppTemplates
//            .FirstOrDefaultAsync(x =>
//                x.TemplateName == templateName &&
//                x.IsActive);
//    }

//    public async Task<List<WhatsAppTemplate>> GetAllActiveAsync()
//    {
//        return await _context.WhatsAppTemplates
//            .Where(x => x.IsActive)
//            .OrderBy(x => x.TemplateName)
//            .ToListAsync();
//    }

//    public async Task AddAsync(WhatsAppTemplate template)
//    {
//        await _context.WhatsAppTemplates.AddAsync(template);
//        await _context.SaveChangesAsync();
//    }

//    public async Task UpdateAsync(WhatsAppTemplate template)
//    {
//        _context.WhatsAppTemplates.Update(template);
//        await _context.SaveChangesAsync();
//    }
//}

using Microsoft.EntityFrameworkCore;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Domain.Entities;
using WhatsAppBulkMessaging.Infrastructure.Data;

namespace WhatsAppBulkMessaging.Infrastructure.Repositories;

public class WhatsAppTemplateRepository : IWhatsAppTemplateRepository
{
    private readonly AppDbContext _context;

    public WhatsAppTemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WhatsAppTemplate>> GetAllAsync()
    {
        return await _context.WhatsAppTemplates
            .Where(x => x.IsActive)
            .OrderBy(x => x.TemplateName)
            .ToListAsync();
    }

    public async Task<WhatsAppTemplate?> GetByNameAsync(string templateName,
        string languageCode)
    {
        return await _context.WhatsAppTemplates
            .FirstOrDefaultAsync(x =>
                x.TemplateName == templateName &&
                x.LanguageCode == languageCode &&
                x.IsActive);
    }

    public async Task AddAsync(WhatsAppTemplate template)
    {
        await _context.WhatsAppTemplates.AddAsync(template);
        await _context.SaveChangesAsync();
    }
}