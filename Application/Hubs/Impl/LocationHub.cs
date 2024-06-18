using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.HubModels;
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
        
        var friends = await _context.UserFriends
            .AsNoTracking()
            .Where(u => u.FriendId == userId)
            .Select(u => new UserLocationModel
            {
                Id = u.UserId,
                UserName = u.User.UserName,
                Avatar = $"{_baseUrlOptions.Url}{u.Friend.AvatarFileName}",
                FullName = u.User.FullName,
                UserStatus = u.User.UserStatus,
                LastOnline = u.User.LastOnline,
                Coordinate = u.IsLocationFrozen ? new CoordinatesModel{Latitude = u.Latitude!.Value, Longitude = u.Longitude!.Value} : new CoordinatesModel{Latitude = u.User.Latitude, Longitude = u.User.Longitude}
            })
            .ToListAsync();

        await Clients.Client(Context.ConnectionId).ReceiveFriendsLocation( _mapper.Map<List<UserLocationModel>>(friends));
        await base.OnConnectedAsync();
    }

    public async Task UpdateLocation(CoordinatesModel coordinates)
    {
        var userId = UserId;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
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
            .Where(u => u.UserId == userId && !u.IsLocationFrozen)
            .Select(u => u.Friend.Id)
            .ToListAsync();
        
        foreach (var id in friendsToNotify)
        {
            var friendConnectionId = await _cache.GetStringAsync(id.ToString());
            if (friendConnectionId == null) continue;
            await Clients.Client(friendConnectionId).ReceiveFriendLocationChanged(userLocationModel);
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