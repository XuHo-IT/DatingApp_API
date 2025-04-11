
using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUser : IdentityUser<int>
{
    public DateOnly DateOfBirth { get; set; }

    public required string KnownAs { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    public required string Gender { get; set; }

    public string? Introduction { get; set; }

    public string? Interests { get; set; }

    public string? LookingFor { get; set; }
    public required string City { get; set; }

    public required string Country { get; set; }

    public List<Photo> Photos { get; set; } = [];

    public List<UserLike> LikeByUsers { get; set; } = [];

    public List<UserLike> LikedUsers { get; set; } = [];

    public List<Messages> MessagesSent { get; set; } = [];

    public List<Messages> MessagesReceived { get; set; } = [];


    public ICollection<AppUserRole> UserRoles { get; set; } = [];

}
