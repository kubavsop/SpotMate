using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.HubModels;
using SpotMate.Application.Services;

namespace SpotMate.Web.Hubs.Impl;

[Authorize]
public sealed class LocationHub: Hub<ILocationHub>
{
    private readonly ILocationService _locationService;

    public LocationHub(ILocationService locationService)
    {

        _locationService = locationService;
    }
    
    public override async Task OnConnectedAsync()
    {
        var friends = await _locationService.HandleOnConnectedAsync(UserId, Context.ConnectionId);
        await Clients.Client(Context.ConnectionId).ReceiveFriendsLocation(friends);
        await base.OnConnectedAsync();
    }

    public async Task UpdateLocation(CoordinatesModel coordinates)
    {
        throw new NotImplementedException();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _locationService.HandleOnDisconnectedAsync(UserId);
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