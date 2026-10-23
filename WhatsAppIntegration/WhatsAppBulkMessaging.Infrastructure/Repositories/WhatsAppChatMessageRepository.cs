
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

    public async Task<WhatsAppChatMessage?> GetByMediaIdAsync(string mediaId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.WhatsAppChatMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MediaId == mediaId);
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
            message.ReadAt ??= DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public async Task UpdateStatusByMetaMessageIdAsync(
        string metaMessageId,
        string status,
        DateTime? statusTime,
        string? errorMessage = null)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var message = await context.WhatsAppChatMessages
            .FirstOrDefaultAsync(x => x.MetaMessageId == metaMessageId);

        if (message is null)
            return;

        var newStatus = status.Trim().ToLowerInvariant();
        var currentStatus = message.Status?.Trim().ToLowerInvariant();

        // Prevent status from moving backwards
        if (currentStatus == "read")
            return;

        if (currentStatus == "delivered" && newStatus == "sent")
            return;

        if (currentStatus == "failed" &&
            (newStatus == "sent" || newStatus == "delivered"))
            return;

        switch (newStatus)
        {
            case "sent":
                message.Status = "Sent";
                message.SentAt ??= statusTime;
                break;

            case "delivered":
                message.Status = "Delivered";
                message.SentAt ??= statusTime;
                message.DeliveredAt ??= statusTime;
                break;

            case "read":
                message.Status = "Read";
                message.SentAt ??= statusTime;
                message.DeliveredAt ??= statusTime;
                message.ReadAt ??= statusTime;
                break;

            case "failed":
                message.Status = "Failed";
                message.ErrorMessage = errorMessage
                    ?? "WhatsApp message delivery failed.";
                break;

            default:
                return;
        }

        await context.SaveChangesAsync();
    }

}
