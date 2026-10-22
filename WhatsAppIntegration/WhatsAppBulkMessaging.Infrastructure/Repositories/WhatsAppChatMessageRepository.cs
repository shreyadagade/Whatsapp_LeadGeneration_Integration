
using Microsoft.EntityFrameworkCore;
using WhatsAppBulkMessaging.Domain.Entities;
using WhatsAppBulkMessaging.Infrastructure.Data;

namespace WhatsAppBulkMessaging.Infrastructure.Repositories;

public class WhatsAppChatMessageRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public WhatsAppChatMessageRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<bool> ExistsAsync(string metaMessageId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.WhatsAppChatMessages
            .AnyAsync(x => x.MetaMessageId == metaMessageId);
    }

    public async Task AddAsync(WhatsAppChatMessage message)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        await context.WhatsAppChatMessages.AddAsync(message);
        await context.SaveChangesAsync();
    }

    public async Task<List<WhatsAppChatMessage>> GetByPhoneNumberAsync(string phoneNumber)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.WhatsAppChatMessages
            .AsNoTracking()
            .Where(x => x.PhoneNumber == phoneNumber)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<WhatsAppChatMessage>> GetAllChatMessagesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.WhatsAppChatMessages
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(string phoneNumber)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var unreadMessages = await context.WhatsAppChatMessages
            .Where(x => x.PhoneNumber == phoneNumber
                        && x.Direction == "Incoming"
                        && !x.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await context.SaveChangesAsync();
    }
}
