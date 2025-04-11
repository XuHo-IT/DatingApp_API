using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper) : BaseAPIController
{
    [HttpPost]
    public async Task<ActionResult<MessagesDto>> CreateMessage(CreateMessagesDto createMessagesDto)
    {
        var username = User.GetUserName();
        if (username == createMessagesDto.RecipientUsername.ToLower())
            return BadRequest("You cannot message youself");

        var sender = await userRepository.GetUserByUserNameAsync(username);
        var recipient = await userRepository.GetUserByUserNameAsync(createMessagesDto.RecipientUsername);

        if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null) return BadRequest("Cannot send message at this time");

        var message = new Messages
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessagesDto.Content
        };
        messageRepository.AddMessage(message);
        if (await messageRepository.SaveAllAsync()) return Ok(mapper.Map<MessagesDto>(message));
        return BadRequest("Failed to save message");
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessagesDto>>> GetMessageForUser([FromQuery] MessagesParam messagesParam)
    {
        messagesParam.Username = User.GetUserName();

        var messages = await messageRepository.GetMessagesForUser(messagesParam);

        Response.AddPaginationHeader(messages);
        return messages;
    }
    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessagesDto>>> GetMessagesThread(string username)
    {
        var currentUsername = User.GetUserName();
        return Ok(await messageRepository.GetMessagesThread(currentUsername, username));
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var userName = User.GetUserName();
        var message = await messageRepository.GetMessages(id);
        if (message == null) return BadRequest("Cannot delete this message");
        if (message.SenderUsername != userName && message.RecipientUsername != userName) return Forbid();
        if (message.SenderUsername == userName) message.SenderDeleted = true;
        if (message.RecipientUsername == userName) message.RecipientDeleted = true;

        if (message is { SenderDeleted: true, RecipientDeleted: true })
        {
            messageRepository.DeleteMessage(message);
        }

        if (await messageRepository.SaveAllAsync()) return Ok();

        return BadRequest("Problem deleting the message");
    }
}