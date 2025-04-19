using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessagesRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddGroup(Group group)
    {
        context.Groups.Add(group);
    }

    public void AddMessage(Messages message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Messages message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await context.Connections.FindAsync(connectionId);
    }

    public async Task<Group?> GetGroupForConnection(string connectionId)
    {
        return await context.Groups
            .Include(x => x.Connections)
            .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
    }

    public async Task<Messages?> GetMessages(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessagesDto>> GetMessagesForUser(MessagesParam messagesParam)
    {
        var query = context.Messages
        .OrderByDescending(x => x.MessageSent)
        .AsQueryable();

        query = messagesParam.Container switch
        {
            "Inbox" => query.Where(x => x.Recipient.UserName == messagesParam.Username
            && x.RecipientDeleted == false),
            "Outbox" => query.Where(x => x.Sender.UserName == messagesParam.Username
            && x.SenderDeleted == false),
            _ => query.Where(x => x.Recipient.UserName == messagesParam.Username && x.DateRead == null
            && x.RecipientDeleted == false)
        };

        var messages = query.ProjectTo<MessagesDto>(mapper.ConfigurationProvider);
        return await PagedList<MessagesDto>.CreateAsync(messages, messagesParam.PageNumber, messagesParam.PageSize);
    }

    public async Task<IEnumerable<MessagesDto>> GetMessagesThread(string currentUsername, string recipientUsername)
    {
        var messages = await context.Messages
        .Where(x =>
           x.RecipientUsername == currentUsername
           && x.RecipientDeleted == false
           && x.SenderUsername == recipientUsername ||

           x.SenderUsername == currentUsername
           && x.SenderDeleted == false
           && x.RecipientUsername == recipientUsername)
           .OrderBy(x => x.MessageSent)
           .ProjectTo<MessagesDto>(mapper.ConfigurationProvider)
           .ToListAsync();

        var unreadMessage = messages.Where(X => X.DateRead == null && X.RecipientUsername == currentUsername).ToList();

        if (unreadMessage.Count != 0)
        {
            unreadMessage.ForEach(x => x.DateRead = DateTime.UtcNow);
        }
        return messages;
    }

    public void RemoveConnection(Connection connection)
    {
        context.Connections.Remove(connection);
    }
}
