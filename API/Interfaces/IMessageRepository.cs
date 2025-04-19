using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Messages message);
    void DeleteMessage(Messages message);
    Task<Messages?> GetMessages(int id);
    Task<PagedList<MessagesDto>> GetMessagesForUser(MessagesParam messagesParam);
    Task<IEnumerable<MessagesDto>> GetMessagesThread(string currentUsername, string recipientUsername);
    Task<bool> SaveAllAsync();
    void AddGroup(Group group);
    void RemoveConnection(Connection connection);
    Task<Connection> GetConnection(string connectionId);
    Task<Group> GetMessageGroup(string groupName);
    Task<Group?> GetGroupForConnection(string connectionId);
}
