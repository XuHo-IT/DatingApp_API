using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignaIR
{
    public class MessageHub(IMessageRepository messageRepository,
        IUserRepository userRepository, IMapper mapper, IHubContext<PrecenseHub> precenseHub) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUSer = httpContext?.Request.Query["user"];

            if (Context.User == null || string.IsNullOrEmpty(otherUSer))
                throw new Exception("Cannot join group");

            var groupName = GetGroupName(Context.User.GetUserName(), otherUSer);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await messageRepository.GetMessagesThread(Context.User.GetUserName(), otherUSer!);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessagesDto createMessagesDto)
        {
            var username = Context.User?.GetUserName() ?? throw new Exception("Could not get user");
            if (username == createMessagesDto.RecipientUsername.ToLower())
                throw new HubException("You cannot message youself");

            var sender = await userRepository.GetUserByUserNameAsync(username);
            var recipient = await userRepository.GetUserByUserNameAsync(createMessagesDto.RecipientUsername);

            if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null)
                throw new HubException("Cannot send message at this time");

            var message = new Messages
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessagesDto.Content
            };
            messageRepository.AddMessage(message);
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await messageRepository.GetMessageGroup(groupName);

            if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null && connections.Count != null)
                {
                    await precenseHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { username = sender.UserName, knowAs = sender.KnownAs });
                }
            }

            if (await messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessagesDto>(message));
            }
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (connection != null && group != null)
            {
                messageRepository.RemoveConnection(connection);
                if (await messageRepository.SaveAllAsync()) return group;
            }
            throw new Exception("Failed to remove from group");
        }
        private async Task<Group> AddToGroup(string groupName)
        {
            var userName = Context.User?.GetUserName() ?? throw new Exception("Cannot get username");
            var group = await messageRepository.GetMessageGroup(groupName);
            var connection = new Connection { ConnectionId = Context.ConnectionId, Username = userName };

            if (group == null)
            {
                group = new Group { Name = groupName };
                messageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);
            if (await messageRepository.SaveAllAsync()) return group;
            throw new HubException("Fail to join group");
        }
        private string GetGroupName(string caller, string? other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

    }
}
