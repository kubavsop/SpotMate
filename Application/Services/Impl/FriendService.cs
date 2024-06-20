using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.HubModels;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Exceptions;
using SpotMate.Application.Hubs;
using SpotMate.Application.Hubs.Impl;
using SpotMate.Application.OperationResult;
using SpotMate.Application.Options;

namespace SpotMate.Application.Services.Impl;

public sealed class FriendService: IFriendService
{
    private readonly BaseUrlOptions _baseUrlOptions;
    private readonly IDistributedCache _cache;
    private readonly IHubContext<LocationHub, ILocationHub> _hubContext; 
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FriendService(IApplicationDbContext context, IMapper mapper, IDistributedCache cache, IHubContext<LocationHub, ILocationHub> hubContext, IOptions<BaseUrlOptions> baseUrlOptions)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _hubContext = hubContext;
        _baseUrlOptions = baseUrlOptions.Value;
    }

    public async Task<Result<IEnumerable<FriendShortDto>>> GetFriendsAsync(UserShortSearchParameters userShortSearchParameters, Guid userId)
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

        var friendsDto = _mapper.Map<List<FriendShortDto>>(friends);

        foreach (var friendShortDto in friendsDto)
        {
            friendShortDto.ChatId =
                (await _context.ChatUsers.FirstOrDefaultAsync(cu => cu.UserId == userId && cu.FriendId == friendShortDto.Id))
                ?.ChatId;
        }

        return friendsDto;
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
            await _hubContext.Clients.Client(friendConnectionId).ReceiveDeletedFriendId(userId);
        }

        if (userConnectionId != null)
        {
            await _hubContext.Clients.Client(userConnectionId).ReceiveDeletedFriendId(friendId);
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
        var freezeLocation =
            await _context.FreezeLocations.FirstOrDefaultAsync(
                fl => fl.UserId == userId && fl.FreezerUserId == friendId);
        friend.IsLocationFrozen = freezeLocation?.IsLocationFrozen ?? false;
        friend.ChatId =
            (await _context.ChatUsers.FirstOrDefaultAsync(cu => cu.UserId == userId && cu.FriendId == friendId))
            ?.ChatId;
        return friend;
    }
    
    public async Task<Result<IEnumerable<UserLocationModel>>> GetFriendsLocation(Guid userId)
    {
        var friends = await _context.UserFriends
            .AsNoTracking()
            .Where(u => u.FriendId == userId)
            .Select(u => new UserLocationModel
            {
                Id = u.UserId,
                UserName = u.User.UserName,
                Avatar = u.User.AvatarFileName != null ? $"{_baseUrlOptions.Url}{u.User.AvatarFileName}" : null,
                FullName = u.User.FullName,
                UserStatus = u.User.UserStatus,
                LastOnline = u.User.LastOnline,
                Coordinate = new CoordinatesModel{Latitude = u.User.Latitude, Longitude = u.User.Longitude},
                ChatId = u.User.ChatUsers.FirstOrDefault(cu => cu.UserId == u.UserId && cu.FriendId == userId) != null ? u.User.ChatUsers.First(cu => cu.UserId == u.UserId && cu.FriendId == userId).ChatId : null
            })
            .ToListAsync();
        
        foreach (var friend in friends)
        {
            var frozenLocation =
                await _context.FreezeLocations.FirstOrDefaultAsync(fl =>
                    fl.UserId == friend.Id && fl.FreezerUserId == userId);

            if (frozenLocation != null && frozenLocation.IsLocationFrozen)
            {
                friend.Coordinate = new CoordinatesModel
                    { Latitude = frozenLocation.Latitude!.Value, Longitude = frozenLocation.Longitude!.Value };
            }
        }
        
        return friends;
    }
}