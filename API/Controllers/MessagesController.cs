using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

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

    if (recipient == null || sender == null) return BadRequest("Cannot send message at this time");

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
  public async Task<ActionResult<IEnumerable<MessagesDto>>> GetMessageForUser([FromQuery]MessagesParam messagesParam)
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
    return Ok(await messageRepository.GetMessagesThread(currentUsername,username));
  }
}