using System;
using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipleExtensions
{
    public static string GetUserName(this ClaimsPrincipal user)
    {
        var username = user.FindFirstValue(ClaimTypes.Name) 
        ?? throw new Exception("Cannot get username from token");
        return username;
    }

    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userid =int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) 
        ?? throw new Exception("Cannot get userid from token"));
        return userid;
    }

}
