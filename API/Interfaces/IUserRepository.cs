using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
   void Update(AppUser user);
   Task<bool> SaveAllAsync();

   Task<IEnumerable<AppUser>> GetUserAsync();
   Task<AppUser?> GetUserByIdAsync(int id);

   Task<AppUser?> GetUserByUserNameAsync(string username);
   Task<PagedList<MemberDto>>  GetMemberAsync(UserParams userParams);

   Task<MemberDto?> GetMembeAsync(string username);
}
