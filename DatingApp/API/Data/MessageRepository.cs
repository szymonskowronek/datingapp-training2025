using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(AppDbContext context) : IMessageRepository
{
    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessage(string messageId)
    {
        return await context.Messages.FindAsync(messageId);
    }

    public async Task<PaginatedResult<MessageDto>> GetMessageForMember(MessageParams messageParams)
    {
        var query = context.Messages
            .OrderByDescending(message => message.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Outbox" => query.Where(message => message.SenderId == messageParams.MemberId 
                && message.SenderDeleted == false),
            _ => query.Where(message => message.RecipientId == messageParams.MemberId && message.RecipientDeleted == false)
        };

        var messageQuery = query.Select(MessageExtensions.ToDtoProjection());

        return await PaginationHelper.CreateAsync(messageQuery, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IReadOnlyList<MessageDto>> GetMessageThred(string currentMemberId, string recipientId)
    {
        await context.Messages
            .Where(message => message.RecipientId == currentMemberId 
                && message.SenderId == recipientId && message.DateRead == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(message => message.DateRead, DateTime.UtcNow));

        return await context.Messages
            .Where(message => (message.RecipientId == currentMemberId 
                    && message.RecipientDeleted == false && message.SenderId == recipientId) 
                || (message.SenderId == currentMemberId 
                    && message.SenderDeleted == false && message.RecipientId == recipientId))
            .OrderBy(message => message.MessageSent)
            .Select(MessageExtensions.ToDtoProjection())
            .ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
