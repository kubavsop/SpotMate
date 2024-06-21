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
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.Services.Impl;

public class UserService: IUserService
{
    private readonly IDistributedCache _cache;
    private readonly IHubContext<LocationHub, ILocationHub> _hubContext; 
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly BaseUrlOptions _baseUrlOptions;

    public UserService(IApplicationDbContext context, IMapper mapper, IDistributedCache cache, IHubContext<LocationHub, ILocationHub> hubContext, IOptions<BaseUrlOptions> baseUrlOptions)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _hubContext = hubContext;
        _baseUrlOptions = baseUrlOptions.Value;
    }

    public async Task<Result<IEnumerable<NonFriendDto>>> GetNonFriendsUsersAsync(UserSearchParameters searchParameters, Guid userId)
    {
        var normalizedUserName = searchParameters.UserName?.ToUpper();

        var myInterests = await _context.UserInterests
            .AsNoTracking()
            .Where(ui => ui.UserId == userId)
            .Select(ui => ui.InterestId)
            .ToListAsync();

        var friends = await _context.UserFriends
            .AsNoTracking()
            .Where(uf => uf.UserId == userId)
            .Select(uf => uf.FriendId)
            .ToListAsync();
        
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id != userId && !friends.Contains(u.Id))
            .Where(u => normalizedUserName == null || u.NormalizedUserName!.Contains(normalizedUserName))
            .Where(u => searchParameters.Interests == null || u.Interests.Select(i => i.Id).Intersect(searchParameters.Interests).Any())
            .Where(u => !searchParameters.IsInterestMatch || myInterests.Count == 0 || u.Interests.Select(i => i.Id).Intersect(myInterests).Any())
            .Select(u => new NonFriendDto
                {
                    Id = u.Id,
                    Avatar = u.AvatarFileName != null ? $"{_baseUrlOptions.Url}{u.AvatarFileName}" : null,
                    FullName = u.FullName,
                    UserName = u.UserName,
                    UserStatus = u.UserStatus,
                    Request = u.ReceivedRequests.Any(r => r.SenderUserId == userId) || u.SentRequests.Any(r => r.ReceiverUserId == userId) ? new ShortRequestDto
                    {
                        HasMyFriendRequest = u.ReceivedRequests.Any(r => r.SenderUserId == userId),
                        RequestStatus = u.ReceivedRequests.Any(r => r.SenderUserId == userId) ? u.ReceivedRequests.First(r => r.SenderUserId == userId).RequestStatus : u.SentRequests.First(r => r.ReceiverUserId == userId).RequestStatus
                    } : null
                }
            )
            .Skip(searchParameters.Offset)
            .Take(searchParameters.Limit)
            .ToListAsync();

        return users;
    }

    public async Task<Result> CreateFriendRequest(Guid senderUserId, Guid receiverUserId)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == senderUserId)|| !await _context.Users.AnyAsync(u => u.Id == receiverUserId))
        {
            return new NotFoundException(nameof(SpotMateUser));
        }

        if (await _context.FriendRequests.AnyAsync(fr =>
                (fr.SenderUserId == senderUserId && fr.ReceiverUserId == receiverUserId) ||
                (fr.SenderUserId == receiverUserId && fr.ReceiverUserId == senderUserId)))
        {
            return new BadRequestException("Request already exists");
        }
        
        if (await _context.UserFriends.AnyAsync(uf => (uf.UserId == senderUserId && uf.FriendId == receiverUserId) || (uf.UserId == receiverUserId && uf.FriendId == senderUserId)))
        {
            return new BadRequestException("You are already friends");
        }

        await _context.FriendRequests.AddAsync(new FriendRequest
        {
            ReceiverUserId = receiverUserId,
            SenderUserId = senderUserId,
            RequestStatus = RequestStatus.Pending
        });
        
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }

    public async Task<Result<UserFullDto>> GetUserByIdAsync(Guid userId, Guid myId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .AsSplitQuery()
            .Include(u => u.Interests)
            .Include(u => u.SentRequests)
            .Include(u => u.ReceivedRequests)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            return new NotFoundException(nameof(SpotMateUser), userId);
        }

        var userFull = _mapper.Map<UserFullDto>(user);
        userFull.Request = user.ReceivedRequests.Any(r => r.SenderUserId == myId) || user.SentRequests.Any(r => r.ReceiverUserId == myId)
                ? new ShortRequestDto
                {
                    HasMyFriendRequest = user.ReceivedRequests.Any(r => r.SenderUserId == myId),
                    RequestStatus = user.ReceivedRequests.Any(r => r.SenderUserId == myId)
                        ? user.ReceivedRequests.First(r => r.SenderUserId == myId).RequestStatus
                        : user.SentRequests.First(r => r.ReceiverUserId == myId).RequestStatus
                }
                : null;
        var freezeLocation =
            await _context.FreezeLocations.FirstOrDefaultAsync(
                fl => fl.UserId == myId && fl.FreezerUserId == userId);
        userFull.IsLocationFrozen = freezeLocation?.IsLocationFrozen ?? false;
        
        return userFull;
    }

    public async Task<Result> DeleteUserRequest(Guid senderUserId, Guid receiverUserId)
    {
        var request = await _context.FriendRequests.FirstOrDefaultAsync(fr => fr.SenderUserId == senderUserId && fr.ReceiverUserId == receiverUserId);
        if (request == null) return new NotFoundException(nameof(FriendRequest));
        
        _context.FriendRequests.Remove(request);

        await _context.SaveChangesAsync();
        
        return Result.Success();
    }

    public async Task<Result> AcceptRequestAsync(Guid userId, Guid myId)
    {
        var request = await _context.FriendRequests.FirstOrDefaultAsync(fr => fr.ReceiverUserId == myId && fr.SenderUserId == userId);
        if (request == null) return new NotFoundException(nameof(FriendRequest));

        await _context.UserFriends.AddAsync(new UserFriend
        {
            UserId = request.SenderUserId,
            FriendId = request.ReceiverUserId
        });
        
        await _context.UserFriends.AddAsync(new UserFriend
        {
            UserId = request.ReceiverUserId,
            FriendId = request.SenderUserId
        });

        var receiverId = request.ReceiverUserId;
        var senderId = request.SenderUserId;
        
        _context.FriendRequests.Remove(request);
        
        await _context.SaveChangesAsync();
        
        var receiverConnectionId = await _cache.GetStringAsync(receiverId.ToString());
        var senderConnectionId = await _cache.GetStringAsync(senderId.ToString());
        
        var flag = false;
        var senderUser = (await _context.Users
            .AsNoTracking()
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == senderId))!;
        var receiverUser = (await _context.Users
            .AsNoTracking()
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == receiverId))!;

        if (senderUser.IsInterestBasedLocationSharable && receiverUser.IsInterestBasedLocationSharable &&
            senderUser.Interests.Select(i => i.Id).Intersect(receiverUser.Interests.Select(i => i.Id)).Any())
        {
            flag = true;
        }
        
        
        if (receiverConnectionId != null)
        {
            var senderUserDto = _mapper.Map<UserLocationModel>(senderUser);
            senderUserDto.Chat = await _context.ChatUsers
                .Where(cu => cu.UserId == receiverId && cu.FriendId == senderId)
                .Select(cu => new ChatShortDto
                {
                    Id = cu.ChatId,
                    Avatar = cu.Friend.AvatarFileName != null ? $"{_baseUrlOptions.Url}{cu.Friend.AvatarFileName}" : null,
                    LastOnline = cu.Friend.LastOnline,
                    Title = cu.Friend.FullName,
                    UserStatus = cu.Friend.UserStatus
                }).FirstOrDefaultAsync();
            
            var frozenLocation =
                await _context.FreezeLocations.FirstOrDefaultAsync(fl =>
                    fl.UserId == senderId && fl.FreezerUserId == receiverId);

            if (frozenLocation != null && frozenLocation.IsLocationFrozen)
            {
                senderUserDto.Coordinate = new CoordinatesModel
                    { Latitude = frozenLocation.Latitude!.Value, Longitude = frozenLocation.Longitude!.Value };
            }

            if (flag)
            {
                await _hubContext.Clients.Client(receiverConnectionId).ReceiveDeletedUserOfSimilarInterestsId(senderId);
            }
            
            await _hubContext.Clients.Client(receiverConnectionId).ReceiveAddedFriend(senderUserDto);
        }

        if (senderConnectionId != null)
        {
            var receiverUserDto = _mapper.Map<UserLocationModel>(receiverUser);
            receiverUserDto.Chat = await _context.ChatUsers
                .Where(cu => cu.UserId == senderId && cu.FriendId == receiverId)
                .Select(cu => new ChatShortDto
                {
                    Id = cu.ChatId,
                    Avatar = cu.Friend.AvatarFileName != null ? $"{_baseUrlOptions.Url}{cu.Friend.AvatarFileName}" : null,
                    LastOnline = cu.Friend.LastOnline,
                    Title = cu.Friend.FullName,
                    UserStatus = cu.Friend.UserStatus
                }).FirstOrDefaultAsync();
            
            var frozenLocation =
                await _context.FreezeLocations.FirstOrDefaultAsync(fl =>
                    fl.UserId == receiverId && fl.FreezerUserId == senderId);

            if (frozenLocation != null && frozenLocation.IsLocationFrozen)
            {
                receiverUserDto.Coordinate = new CoordinatesModel
                    { Latitude = frozenLocation.Latitude!.Value, Longitude = frozenLocation.Longitude!.Value };
            }
            
            if (flag)
            {
                await _hubContext.Clients.Client(senderConnectionId).ReceiveDeletedUserOfSimilarInterestsId(receiverId);
            }
            
            await _hubContext.Clients.Client(senderConnectionId).ReceiveAddedFriend(receiverUserDto);
        }

        return Result.Success();
    }

    public async Task<Result> DeclineRequestAsync(Guid userId, Guid myId)
    {
        var request = await _context.FriendRequests.FirstOrDefaultAsync(fr => fr.ReceiverUserId == myId && fr.SenderUserId == userId);
        if (request == null) return new NotFoundException(nameof(FriendRequest));

        request.RequestStatus = RequestStatus.Declined;

        await _context.SaveChangesAsync();
        
        return Result.Success();    
    }
    
    public async Task<Result> FreezeLocationAsync(Guid freezerUserId, Guid userId)
    {
        var freezeLocation = await _context.FreezeLocations
            .Include(fl => fl.User)
            .FirstOrDefaultAsync(fl => fl.UserId == userId && fl.FreezerUserId == freezerUserId);
        
        if (freezeLocation == null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return new NotFoundException(nameof(SpotMateUser));

            await _context.FreezeLocations.AddAsync(new FreezeLocation
            {
                UserId = userId,
                FreezerUserId = freezerUserId,
                Longitude = user.Longitude,
                Latitude = user.Latitude,
                IsLocationFrozen = true
            });
            await _context.SaveChangesAsync();
            
            return Result.Success();
        }
        
        if (freezeLocation.IsLocationFrozen)
        {
            return new BadRequestException("The location is already frozen");
        }

        freezeLocation.Latitude = freezeLocation.User.Latitude;
        freezeLocation.Longitude = freezeLocation.User.Longitude;
        freezeLocation.IsLocationFrozen = true;
        await _context.SaveChangesAsync();

        return Result.Success();    
    }

    public async Task<Result> UnFreezeLocationAsync(Guid freezerUserId, Guid userId)
    {
        var freezeLocation = await _context.FreezeLocations
            .FirstOrDefaultAsync(fl => fl.UserId == userId && fl.FreezerUserId == freezerUserId);
        
        if (freezeLocation is not { IsLocationFrozen: true })
        {
            return new BadRequestException("The location is already unfrozen");
        }

        freezeLocation.Latitude = null;
        freezeLocation.Longitude = null;
        freezeLocation.IsLocationFrozen = false;
        await _context.SaveChangesAsync();

        return Result.Success();    
    }

    public async Task<Result<IEnumerable<UserShortLocationModel>>> GetInterestBaseLocations(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null) return new NotFoundException(nameof(SpotMateUser), userId);
        if (!user.IsInterestBasedLocationSharable)
            return new BadRequestException("IsInterestBasedLocationSharable must be true");
        
        
        var friends = await _context.UserFriends
            .AsNoTracking()
            .Where(uf => uf.UserId == userId)
            .Select(uf => uf.FriendId)
            .ToListAsync();
        
        var interestsId = user.Interests.Select(i => i.Id).ToList();
        
        var users = await _context.Users
            .AsNoTracking()
            .Where(u =>
                u.Id != userId && u.IsInterestBasedLocationSharable && !friends.Contains(u.Id) && u.Interests.Select(i => i.Id).Intersect(interestsId).Any())
            .Select(u => new UserShortLocationModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Avatar = u.AvatarFileName != null ? $"{_baseUrlOptions.Url}{u.AvatarFileName}" : null,
                FullName = u.FullName,
                UserStatus = u.UserStatus,
                LastOnline = u.LastOnline,
                Coordinate = new CoordinatesModel{Latitude = u.Latitude, Longitude = u.Longitude},
            })
            .ToListAsync();
        
        
        foreach (var userLocationModel in users)
        {
            var frozenLocation =
                await _context.FreezeLocations.FirstOrDefaultAsync(fl =>
                    fl.UserId == userLocationModel.Id && fl.FreezerUserId == userId);

            if (frozenLocation != null && frozenLocation.IsLocationFrozen)
            {
                userLocationModel.Coordinate = new CoordinatesModel
                    { Latitude = frozenLocation.Latitude!.Value, Longitude = frozenLocation.Longitude!.Value };
            }
        }

        return users;
    }
 
}