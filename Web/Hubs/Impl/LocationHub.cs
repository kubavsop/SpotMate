using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SpotMate.Application.Context;
using SpotMate.Web.Hubs.Models;

namespace SpotMate.Web.Hubs.Impl;

[Authorize]
public sealed class LocationHub: Hub<ILocationHub>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;


    public LocationHub(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = UserId;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return;
        }

        user.LastOnline = null;
        await _context.SaveChangesAsync();
        
        var friends = await _context.UserFriends
            .Where(u => u.UserId == userId)
            .Select(u => u.Friend)
            .ToListAsync();

        await Clients.Client(Context.ConnectionId).GetFriendsLocation(_mapper.Map<List<UserLocationModel>>(friends));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);

        if (user == null)
        {
            return;
        }
        
        user.LastOnline = DateTime.UtcNow;
        await _context.SaveChangesAsync();
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