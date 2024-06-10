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
            .Include( uf => uf.FirstUser)
            .Include(uf => uf.SecondUser)
            .Where(uf => uf.SecondUserId == userId || uf.FirstUserId == userId)
            .Where(uf => normalizedUserName == null ||
                         (uf.FirstUserId == userId && uf.SecondUser.UserName == normalizedUserName) ||
                         (uf.SecondUserId == userId && uf.FirstUser.UserName == normalizedUserName))
            .Select(uf => uf.FirstUserId == userId ? uf.SecondUser : uf.FirstUser)
            .ToListAsync();

        return _mapper.Map<List<UserShortDto>>(friends);
    }

    public async Task<Result> DeleteFriendAsync(Guid friendId, Guid userId)
    {
        var userFriend = await _context.UserFriends.FirstOrDefaultAsync(uf =>
            (uf.FirstUserId == friendId && uf.SecondUserId == userId) ||
            (uf.SecondUserId == friendId && uf.FirstUserId == userId));

        if (userFriend == null)
        {
            return new BadRequestException("The user is not your friend");
        }

        _context.UserFriends.Remove(userFriend);
        
        return Result.Success();
    }
}