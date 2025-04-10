using System;

namespace API.DTOs;

public class CreateMessagesDto
{
   public required string RecipientUsername { get; set; }
   public required string Content { get; set; }
}
