using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.HubModels;

namespace SpotMate.Application.Services.Impl;

public sealed class LocationService: ILocationService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public LocationService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserLocationModel>> HandleOnConnectedAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return [];
        }

        user.LastOnline = null;
        await _context.SaveChangesAsync();
        
        var friends = await _context.UserFriends
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => u.Friend)
            .ToListAsync();

        return _mapper.Map<List<UserLocationModel>>(friends);
    }

    public async Task HandleOnDisconnectedAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return;
        }
        
        user.LastOnline = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public Task<IEnumerable<Guid>> GetFriendsToNotifyAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateUserLocationAsync(Guid userId, CoordinatesModel coordinatesModel)
    {
        throw new NotImplementedException();
    }
}