using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(DataContext context, IMapper mapper) : ILikeRepository
{
    public void AddLike(UserLike like)
    {
        context.Like.Add(like);
    }

    public void DeleteLike(UserLike like)
    {
         context.Like.Remove(like);
    }

    public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId)
    {
        return await context.Like
        .Where(x => x.SourceUserId == currentUserId)
        .Select(x => x.TargetUserId)
        .ToListAsync();
    }

    public async Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await context.Like.FindAsync(sourceUserId,targetUserId);
    }

    public async Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams)
    {
        var likes = context.Like.AsQueryable();
        IQueryable<MemberDto> query;

        switch(likesParams.Predicate)
        {
            case "liked":
            query = likes
            .Where(x => x.SourceUserId == likesParams.UserId)
            .Select(x => x.TargetUser)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
            break;

            case "likedBy":
            query = likes
            .Where(x => x.TargetUserId == likesParams.UserId)
            .Select(x => x.SourceUser)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
            break;

            default:
            var likeIds = await GetCurrentUserLikeIds(likesParams.UserId);

            query = likes
            .Where(x => x.TargetUserId ==likesParams.UserId && likeIds.Contains(x.SourceUserId))
            .Select(x => x.SourceUser)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
            break;
        }
        return await PagedList<MemberDto>.CreateAsync(query, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<bool> SaveChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
