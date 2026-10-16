using Microsoft.EntityFrameworkCore;
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Domain.Entities;
using WhatsAppBulkMessaging.Infrastructure.Data;

namespace WhatsAppBulkMessaging.Infrastructure.Repositories;

public class WhatsAppMessageRepository : IWhatsAppMessageRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public WhatsAppMessageRepository(
        IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddAsync(WhatsAppMessage message)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        await context.WhatsAppMessages.AddAsync(message);
        await context.SaveChangesAsync();
    }

    public async Task<WhatsAppMessage?> GetByMetaMessageIdAsync(
        string metaMessageId)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.WhatsAppMessages
            .FirstOrDefaultAsync(x =>
                x.MetaMessageId == metaMessageId);
    }

    public async Task UpdateAsync(WhatsAppMessage message)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        context.WhatsAppMessages.Update(message);
        await context.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(
        string metaMessageId,
        string status,
        DateTime? statusTime,
        string? errorMessage = null)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var message = await context.WhatsAppMessages
            .FirstOrDefaultAsync(x =>
                x.MetaMessageId == metaMessageId);

        if (message is null)
            return;

        var newStatus = status.Trim().ToLowerInvariant();

        var currentStatus = message.Status?
            .Trim()
            .ToLowerInvariant();

        // Do not downgrade an already read message.
        if (currentStatus == "read")
            return;

        // Do not change a delivered message back to sent.
        if (currentStatus == "delivered" &&
            newStatus == "sent")
        {
            return;
        }

        // Do not replace a failed status with an earlier status.
        if (currentStatus == "failed" &&
            (newStatus == "sent" ||
             newStatus == "delivered"))
        {
            return;
        }

        switch (newStatus)
        {
            case "sent":
                message.Status = "sent";
                message.SentAt ??= statusTime;
                break;

            case "delivered":
                message.Status = "delivered";
                message.SentAt ??= statusTime;
                message.DeliveredAt ??= statusTime;
                break;

            case "read":
                message.Status = "read";
                message.SentAt ??= statusTime;
                message.DeliveredAt ??= statusTime;
                message.ReadAt ??= statusTime;
                break;

            case "failed":
                message.Status = "failed";
                message.FailedAt ??= statusTime;
                message.ErrorMessage = errorMessage;
                break;

            default:
                return;
        }

        message.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task<WhatsAppMessage?> GetLatestByPhoneNumberAsync(
        string phoneNumber)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.WhatsAppMessages
            .Where(x => x.PhoneNumber == phoneNumber)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateReplyAsync(
        long messageId,
        string replyMessage,
        DateTime replyReceivedAt)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        var message = await context.WhatsAppMessages
            .FirstOrDefaultAsync(x => x.Id == messageId);

        if (message is null)
            return;

        message.ReplyMessage = replyMessage;
        message.ReplyReceivedAt = replyReceivedAt;
        message.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task<List<WhatsAppMessage>> GetAllAsync()
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.WhatsAppMessages
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<WhatsAppMessage>> GetByPhoneNumberAsync(string phoneNumber)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        return await context.WhatsAppMessages
            .AsNoTracking()
            .Where(x => x.PhoneNumber.EndsWith(phoneNumber))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddIncomingReplyAsync(WhatsAppReplyDto reply)
    {
        if (string.IsNullOrWhiteSpace(reply.MetaMessageId))
            return;

        await using var context =
            await _contextFactory.CreateDbContextAsync();

        // Prevent duplicate replies
        var alreadyExists = await context.WhatsAppReplies
            .AnyAsync(x => x.MetaMessageId == reply.MetaMessageId);

        if (alreadyExists)
            return;

        var entity = new WhatsAppReply
        {
            WhatsAppMessageId = reply.WhatsAppMessageId,
            PhoneNumber = reply.PhoneNumber,
            ReplyMessage = reply.ReplyMessage,
            MetaMessageId = reply.MetaMessageId,
            ReplyReceivedAt = reply.ReplyReceivedAt
        };

        context.WhatsAppReplies.Add(entity);

        await context.SaveChangesAsync();
    }
}
