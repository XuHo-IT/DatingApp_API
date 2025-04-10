using System;

namespace API.Helpers;

public class MessagesParam : PaginationParams
{
  public  string? Username { get; set; }
  public string Container { get; set; } = "Unread";
}
