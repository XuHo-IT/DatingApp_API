using System;
using API.Helpers;

namespace API.Extensions;

public class UserParams : PaginationParams
{
  
   public string? Gender { get; set; }

   public string? CurrentUsername { get; set; }
   
   public int MinAge { get; set; } = 18;
    
    public int MaxAge { get; set; } = 100;

    public string OrderBy { get; set; } ="lastActive";

}
