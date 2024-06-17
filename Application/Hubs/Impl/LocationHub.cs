using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.HubModels;

namespace SpotMate.Application.Hubs.Impl;

public sealed class LocationHub: Hub<ILocationHub>
{
    private const string BaseUrl = "http://89.111.175.47:8080/static/";
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public LocationHub(IDistributedCache cache, IApplicationDbContext context, IMapper mapper)
    {
        _cache = cache;
        _context = context;
        _mapper = mapper;
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
            await Clients.Client(friendConnectionId).ReceiveFriendLocationChangedAsync(userLocationModel);
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