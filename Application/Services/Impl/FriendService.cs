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
            friendShortDto.Chat =
                await _context.ChatUsers
                    .Where(cu => cu.UserId == userId && cu.FriendId == friendShortDto.Id)
                    .Select(cu => new ChatShortDto
                    {
                        Id = cu.ChatId,
                        Avatar = cu.Friend.AvatarFileName != null ? $"{_baseUrlOptions.Url}{cu.Friend.AvatarFileName}" : null,
                        LastOnline = cu.Friend.LastOnline,
                        Title = cu.Friend.FullName,
                        UserStatus = cu.Friend.UserStatus
                    }).FirstOrDefaultAsync();
        }

        return friendsDto;
    }

    public async Task<Result> DeleteFriendAsync(Guid friendId, Guid userId)
    {
        var firstUserFriend = await _context.UserFriends
            .Include(uf => uf.User)
            .ThenInclude(u => u.Interests)
            .FirstOrDefaultAsync(uf => uf.UserId == userId &&
                                                                              uf.FriendId == friendId);
        var secondUserFriend = await _context.UserFriends
            .Include(uf => uf.User)
            .ThenInclude(u => u.Interests)
            .FirstOrDefaultAsync(uf => uf.UserId == friendId &&
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

        var flag = false;
        var user = firstUserFriend.User;
        var friend = secondUserFriend.User;
        if (user.IsInterestBasedLocationSharable && friend.IsInterestBasedLocationSharable &&
            user.Interests.Select(i => i.Id).Intersect(friend.Interests.Select(i => i.Id)).Any())
        {
            flag = true;
        }
        
        if (friendConnectionId != null)
        {
            
            await _hubContext.Clients.Client(friendConnectionId).ReceiveDeletedFriendId(userId);

            if (flag)
            {
                var frozenLocation =
                    await _context.FreezeLocations.FirstOrDefaultAsync(f =>
                        f.UserId == userId && f.FreezerUserId == friendId);
            
                var userLocationModel = new UserShortLocationModel
                {
                    Id = user.Id,
                    Avatar = user.AvatarFileName != null ? $"{_baseUrlOptions.Url}{user.AvatarFileName}" : null,
                    Coordinate = frozenLocation is { IsLocationFrozen: true } ? new CoordinatesModel
                        { Latitude = frozenLocation.Latitude!.Value, Longitude = frozenLocation.Longitude!.Value } : new CoordinatesModel { Latitude = user.Latitude, Longitude = user.Longitude },
                    FullName = user.FullName,
                    LastOnline = user.LastOnline,
                    UserName = user.UserName,
                    UserStatus = user.UserStatus
                };
            
                await _hubContext.Clients.Client(friendConnectionId).ReceiveAddedUserOfSimilarInterests(userLocationModel);
            }
        }

        if (userConnectionId != null)
        { 
            await _hubContext.Clients.Client(userConnectionId).ReceiveDeletedFriendId(friendId);
            
            if (flag)
            {
                var frozenLocation =
                    await _context.FreezeLocations.FirstOrDefaultAsync(f =>
                        f.UserId == friendId && f.FreezerUserId == userId);
            
                var friendLocationModel = new UserShortLocationModel
                {
                    Id = friend.Id,
                    Avatar = friend.AvatarFileName != null ? $"{_baseUrlOptions.Url}{friend.AvatarFileName}" : null,
                    Coordinate = frozenLocation is { IsLocationFrozen: true } ? new CoordinatesModel
                        { Latitude = frozenLocation.Latitude!.Value, Longitude = frozenLocation.Longitude!.Value } : new CoordinatesModel { Latitude = friend.Latitude, Longitude = friend.Longitude },
                    FullName = friend.FullName,
                    LastOnline = friend.LastOnline,
                    UserName = friend.UserName,
                    UserStatus = friend.UserStatus
                };
            
                await _hubContext.Clients.Client(userConnectionId).ReceiveAddedUserOfSimilarInterests(friendLocationModel);
            }
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
        friend.Chat =
            await _context.ChatUsers
                .Where(cu => cu.UserId == userId && cu.FriendId == friendId)
                .Select(cu => new ChatShortDto
                {
                    Id = cu.ChatId,
                    Avatar = cu.Friend.AvatarFileName != null ? $"{_baseUrlOptions.Url}{cu.Friend.AvatarFileName}" : null,
                    LastOnline = cu.Friend.LastOnline,
                    Title = cu.Friend.FullName,
                    UserStatus = cu.Friend.UserStatus
                }).FirstOrDefaultAsync();
        return friend;
    }
    
    public async Task<Result<IEnumerable<UserLocationModel>>> GetFriendsLocation(Guid userId)
    {
        var friends = await _context.UserFriends
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => new UserLocationModel
            {
                Id = u.FriendId,
                UserName = u.Friend.UserName,
                Avatar = u.Friend.AvatarFileName != null ? $"{_baseUrlOptions.Url}{u.Friend.AvatarFileName}" : null,
                FullName = u.Friend.FullName,
                UserStatus = u.Friend.UserStatus,
                LastOnline = u.Friend.LastOnline,
                Coordinate = new CoordinatesModel{Latitude = u.Friend.Latitude, Longitude = u.Friend.Longitude},
                Chat = u.User.ChatUsers.Any(cu => cu.UserId == userId && cu.FriendId == u.FriendId) ? 
                    u.User.ChatUsers.Where(cu => cu.UserId == userId && cu.FriendId == u.FriendId).Select(cu => new ChatShortDto
                    {
                        Id = cu.ChatId,
                        Avatar = cu.Friend.AvatarFileName != null ? $"{_baseUrlOptions.Url}{cu.Friend.AvatarFileName}" : null,
                        LastOnline = cu.Friend.LastOnline,
                        Title = cu.Friend.FullName,
                        UserStatus = cu.Friend.UserStatus
                    }).FirstOrDefault() : null
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