using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.HubModels;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Options;

namespace SpotMate.Application.Hubs.Impl;

[Authorize]
public sealed class LocationHub: Hub<ILocationHub>
{
    private readonly BaseUrlOptions _baseUrlOptions;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public LocationHub(IDistributedCache cache, IApplicationDbContext context, IMapper mapper, IOptions<BaseUrlOptions> baseUrlOptions)
    {
        _cache = cache;
        _context = context;
        _mapper = mapper;
        _baseUrlOptions = baseUrlOptions.Value;
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = UserId;
        await _cache.SetStringAsync(userId.ToString(), Context.ConnectionId);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return;
        }

        user.LastOnline = null;
        await _context.SaveChangesAsync();
        
        await base.OnConnectedAsync();
    }

    public async Task UpdateLocation(CoordinatesModel coordinates)
    {
        var userId = UserId;
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null || user.IsInvisible)
        {
            return;
        }

        user.Latitude = coordinates.Latitude;
        user.Longitude = coordinates.Longitude;
        await _context.SaveChangesAsync();

        var userLocationModel = _mapper.Map<UserLocationModel>(user);
        var friendsToNotify = await _context.UserFriends
            .AsNoTracking()
            .Where(u => u.UserId == userId && !u.User.UserLocations.Any(fl => fl.IsLocationFrozen && fl.FreezerUserId == u.FriendId))
            .Select(u => u.Friend.Id)
            .ToListAsync();
        
        foreach (var id in friendsToNotify)
        {
            var friendConnectionId = await _cache.GetStringAsync(id.ToString());
            if (friendConnectionId == null) continue;
            userLocationModel.Chat = await _context.ChatUsers
                .Where(cu => cu.UserId == userId && cu.FriendId == id)
                .Select(cu => new ChatShortDto
                {
                    Id = cu.ChatId,
                    Avatar = cu.Friend.AvatarFileName != null
                        ? $"{_baseUrlOptions.Url}{cu.Friend.AvatarFileName}"
                        : null,
                    LastOnline = cu.Friend.LastOnline,
                    Title = cu.Friend.FullName,
                    UserStatus = cu.Friend.UserStatus
                }).FirstOrDefaultAsync();
            
            await Clients.Client(friendConnectionId).ReceiveFriendLocationChanged(userLocationModel);
        }

        var interestsId = user.Interests.Select(i => i.Id);
        var usersToNotify = await _context.Users
            .AsNoTracking()
            .Where(u =>
                u.Id != userId && u.IsInterestBasedLocationSharable && !friendsToNotify.Contains(u.Id) &&
                u.Interests.Select(i => i.Id).Intersect(interestsId).Any() && !u.FreezerUserLocations.Any(fl => fl.IsLocationFrozen && fl.UserId == userId))
            .Select(u => u.Id)
            .ToListAsync();

        var userShortLocationModel = new UserShortLocationModel
        {
            Id = userLocationModel.Id,
            UserName = userLocationModel.UserName,
            Avatar = userLocationModel.Avatar,
            FullName = userLocationModel.FullName,
            UserStatus = userLocationModel.UserStatus,
            LastOnline = userLocationModel.LastOnline,
            Coordinate = userLocationModel.Coordinate,
        };

        foreach (var id in usersToNotify)
        {
            var userToNotifyConnectionId = await _cache.GetStringAsync(id.ToString());
            if (userToNotifyConnectionId == null) continue;
            await Clients.Client(userToNotifyConnectionId)
                .ReceiveUserOfSimilarInterestsLocationChanged(userShortLocationModel);
        }
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = UserId;
        await _cache.RemoveAsync(userId.ToString());
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return;
        }
        
        user.LastOnline = DateTime.UtcNow;
        await _context.SaveChangesAsync();        
        await base.OnDisconnectedAsync(exception);
    }
    
    private Guid UserId
    {
        get
        {
            var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Context.User?.Identity?.IsAuthenticated == null || value == null
                ? Guid.Empty
                : Guid.Parse(value);
        }
    }
}