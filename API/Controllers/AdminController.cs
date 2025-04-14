using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace API.Controllers
{
    public class AdminController(UserManager<AppUser> userManager) : BaseAPIController
    {
        [Authorize(Policy = "RequiredAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var user = await userManager.Users
                .OrderBy(x => x.UserName)
                .Select(x =>
                new
                {
                    x.Id,
                    Username = x.UserName,
                    Roles = x.UserRoles.Select(x => x.Role.Name).ToList()
                }).ToListAsync();
            return Ok(user);
        }

        [Authorize(Policy = ("RequiredAdminRole"))]
        [HttpGet("edit-role/{username}")]
        public async Task<ActionResult> EditRoles(string username, string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

            var selectedRoles = roles.Split(",").ToArray();

            var user = await userManager.FindByNameAsync(username);

            if (user == null) return BadRequest("User not found");

            var userRoles = await userManager.GetRolesAsync(user);

            var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add role");

            result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) return BadRequest("Failed to remove role");

            return Ok(await userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratorPhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("abc moderator");
        }
    }
}
