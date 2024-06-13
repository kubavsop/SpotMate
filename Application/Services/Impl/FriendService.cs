using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Exceptions;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services.Impl;

public sealed class FriendService: IFriendService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FriendService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<UserShortDto>>> GetFriendsAsync(UserShortSearchParameters userShortSearchParameters, Guid userId)
    {
        var normalizedUserName = userShortSearchParameters.UserName?.ToUpper();

        var friends = await _context.UserFriends
            .Include(f => f.Friend)
            .Where(f => f.UserId == userId)
            .Where(uf => normalizedUserName == null || uf.Friend.NormalizedUserName!.Contains(normalizedUserName))
            .Skip(userShortSearchParameters.Offset)
            .Take(userShortSearchParameters.Limit)
            .Select(f => f.Friend)
            .ToListAsync();

        return _mapper.Map<List<UserShortDto>>(friends);
    }

    public async Task<Result> DeleteFriendAsync(Guid friendId, Guid userId)
    {
        var firstUserFriend = await _context.UserFriends.FirstOrDefaultAsync(uf => uf.UserId == userId &&
                                                                              uf.FriendId == friendId);
        var secondUserFriend = await _context.UserFriends.FirstOrDefaultAsync(uf => uf.UserId == friendId &&
            uf.FriendId == userId);

        if (firstUserFriend == null || secondUserFriend == null)
        {
            return new BadRequestException("The user is not your friend");
        }

        _context.UserFriends.Remove(firstUserFriend);
        _context.UserFriends.Remove(secondUserFriend);

        await _context.SaveChangesAsync();
        return Result.Success();
    }
}