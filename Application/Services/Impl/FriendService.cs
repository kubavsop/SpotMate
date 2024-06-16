using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Exceptions;
using SpotMate.Application.Hubs;
using SpotMate.Application.Hubs.Impl;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services.Impl;

public sealed class FriendService: IFriendService
{
    private readonly IDistributedCache _cache;
    private readonly IHubContext<LocationHub, ILocationHub> _hubContext; 
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FriendService(IApplicationDbContext context, IMapper mapper, IDistributedCache cache, IHubContext<LocationHub, ILocationHub> hubContext)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _hubContext = hubContext;
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
        
        var friendConnectionId = await _cache.GetStringAsync(friendId.ToString());
        var userConnectionId = await _cache.GetStringAsync(userId.ToString());

        if (friendConnectionId != null)
        {
            await _hubContext.Clients.Client(friendConnectionId).ReceiveDeletedFriendIdAsync(userId);
        }

        if (userConnectionId != null)
        {
            await _hubContext.Clients.Client(userConnectionId).ReceiveDeletedFriendIdAsync(friendId);
        }
        
        return Result.Success();
    }

    public async Task<Result<FriendDto>> GetFriend(Guid friendId, Guid userId)
    {
        var userFriend = await _context.UserFriends
            .AsNoTracking()
            .Include(uf => uf.Friend)
            .ThenInclude(f => f.Interests)
            .FirstOrDefaultAsync(uf => uf.FriendId == friendId && uf.UserId == userId);

        if (userFriend == null)
        {
            return new BadRequestException("The user is not your friend");
        }

        var friend = _mapper.Map<FriendDto>(userFriend.Friend);
        friend.IsLocationFrozen = userFriend.IsLocationFrozen;
        return friend;
    }

    public async Task<Result> FreezeLocationAsync(Guid friendId, Guid userId)
    {
        var userFriend = await _context.UserFriends
            .Include(uf => uf.User)
            .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.FriendId == friendId);
        
        if (userFriend == null)
        {
            return new BadRequestException("The user is not your friend");
        }
        
        if (userFriend.IsLocationFrozen)
        {
            return new BadRequestException("The location is already frozen");
        }

        userFriend.Latitude = userFriend.User.Latitude;
        userFriend.Longitude = userFriend.User.Longitude;
        userFriend.IsLocationFrozen = true;
        await _context.SaveChangesAsync();

        return Result.Success();    
    }

    public async Task<Result> UnFreezeLocationAsync(Guid friendId, Guid userId)
    {
        var userFriend = await _context.UserFriends
            .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.FriendId == friendId);
        
        if (userFriend == null)
        {
            return new BadRequestException("The user is not your friend");
        }

        if (!userFriend.IsLocationFrozen)
        {
            return new BadRequestException("The location is already unfrozen");
        }

        userFriend.Latitude = null;
        userFriend.Longitude = null;
        userFriend.IsLocationFrozen = false;
        await _context.SaveChangesAsync();

        return Result.Success();    
    }
    
}