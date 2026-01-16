using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(AppDbContext context) : ILikesRepository
{
    public void AddLike(MemberLike like)
    {
        context.Likes.Add(like);
    }

    public void DeleteLike(MemberLike like)
    {
        context.Remove(like);
    }

    public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId)
    {
        return await context.Likes
            .Where(memberLike => memberLike.SourceMemberId == memberId)
            .Select(memberLike => memberLike.TargetMemberId)
            .ToListAsync();
    }

    public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId)
    {
        return await context.Likes.FindAsync(sourceMemberId, targetMemberId);
    }

    public async Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams)
    {
        var query = context.Likes.AsQueryable();
        IQueryable<Member> result;

        switch (likesParams.Predicate)
        {
            case "liked":
                result = query
                    .Where(memberLike => memberLike.SourceMemberId == likesParams.CurrentMemberId)
                    .Select(memberLike => memberLike.TargetMember);
                    break;
            case "likedBy":
                result = query
                    .Where(memberLike => memberLike.TargetMemberId == likesParams.CurrentMemberId)
                    .Select(memberLike => memberLike.SourceMember);
                    break;
            default: //mutual
                var likeIds = await GetCurrentMemberLikeIds(likesParams.CurrentMemberId);

                result =  query
                    .Where(memberLike => memberLike.TargetMemberId == likesParams.CurrentMemberId
                        && likeIds.Contains(memberLike.SourceMemberId))
                    .Select(memberLike => memberLike.SourceMember);
                    break;
        }

        return await PaginationHelper.CreateAsync(result, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<bool> SaveAllChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
