using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
     private readonly ILogger<UserRepository> logger;
    public async Task<MemberDto?> GetMembeAsync(string username)
    {
        return await context.Users
        .Where(x=>x.UserName ==username)
        .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMemberAsync(UserParams userParams)
    {
        var querry = context.Users.AsQueryable();

        querry = querry.Where(x =>x.UserName != userParams.CurrentUsername);

        if (userParams.Gender != null)
        {
            querry = querry.Where(x=>x.Gender == userParams.Gender);
        }

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge-1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        querry = querry.Where(x =>x.DateOfBirth >= minDob && x.DateOfBirth <=maxDob);

        querry = userParams.OrderBy switch
        {
            "created" => querry.OrderByDescending(x => x.Created),
            _ =>querry.OrderByDescending(x=>x.LastActive),
        };

        return await PagedList<MemberDto>.CreateAsync(querry.ProjectTo<MemberDto>(mapper.ConfigurationProvider),
        userParams.PageNumber,userParams.PageSize);
        
    }

    public async Task<IEnumerable<AppUser>> GetUserAsync()
    {
        return await context.Users
        .Include(x=>x.Photos)
        .ToListAsync();
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetUserByUserNameAsync(string username)
    {
        return await context.Users
        .Include(x=>x.Photos)
        .SingleOrDefaultAsync(x=>x.UserName==username);
    }

   public async Task<bool> SaveAllAsync()
{
    try
    {
        return await context.SaveChangesAsync() > 0;
    }
    catch (DbUpdateException ex)
    {
        // Log the exception message and inner exception
        logger.LogError(ex, "An error occurred while saving changes to the database.");
        throw; // Re-throw the exception after logging
    }
}


    public void Update(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }
}
