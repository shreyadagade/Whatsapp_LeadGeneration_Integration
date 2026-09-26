//using Microsoft.EntityFrameworkCore;
//using WhatsAppBulkMessaging.Application.Interfaces;
//using WhatsAppBulkMessaging.Domain.Entities;
//using WhatsAppBulkMessaging.Infrastructure.Data;

//namespace WhatsAppBulkMessaging.Infrastructure.Repositories;

//public class WhatsAppMessageRepository : IWhatsAppMessageRepository
//{
//    private readonly AppDbContext _context;
//    public WhatsAppMessageRepository(AppDbContext context)
//    {
//        _context = context;
//    }

//    public async Task AddAsync(WhatsAppMessage message)
//    {
//        await _context.WhatsAppMessages.AddAsync(message);
//        await _context.SaveChangesAsync();
//    }

//    public async Task UpdateStatusAsync(
//        string metaMessageId,
//        string status,
//        string? failureReason = null)
//    {
//        var message = await _context.WhatsAppMessages
//            .FirstOrDefaultAsync(x => x.MetaMessageId == metaMessageId);

//        if (message == null)
//            return;

//        message.Status = status;
//        message.FailureReason = failureReason;

//        if (status.Equals("failed", StringComparison.OrdinalIgnoreCase))
//        {
//            message.UserFriendlyFailureReason =
//                "Message could not be delivered to this recipient. Please try again later.";
//        }
//        else
//        {
//            message.UserFriendlyFailureReason = null;
//        }

//        message.UpdatedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync();
//    }
//}

using Microsoft.EntityFrameworkCore;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Domain.Entities;
using WhatsAppBulkMessaging.Infrastructure.Data;

namespace WhatsAppBulkMessaging.Infrastructure.Repositories;

public class WhatsAppMessageRepository : IWhatsAppMessageRepository
{
    private readonly AppDbContext _context;

    public WhatsAppMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(WhatsAppMessage message)
    {
        await _context.WhatsAppMessages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task<WhatsAppMessage?> GetByMetaMessageIdAsync(
        string metaMessageId)
    {
        return await _context.WhatsAppMessages
            .FirstOrDefaultAsync(x =>
                x.MetaMessageId == metaMessageId);
    }

    public async Task UpdateAsync(WhatsAppMessage message)
    {
        _context.WhatsAppMessages.Update(message);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(
        string metaMessageId,
        string status)
    {
        var message = await _context.WhatsAppMessages
            .FirstOrDefaultAsync(x =>
                x.MetaMessageId == metaMessageId);

        if (message is null)
        {
            return;
        }

        message.Status = status;
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}

