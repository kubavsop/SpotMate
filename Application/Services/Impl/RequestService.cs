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

public sealed class RequestService: IRequestService
{
    private readonly IDistributedCache _cache;
    private readonly IHubContext<LocationHub, ILocationHub> _hubContext; 
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly BaseUrlOptions _baseUrlOptions;

    public RequestService(IApplicationDbContext context, IMapper mapper, IDistributedCache cache, IHubContext<LocationHub, ILocationHub> hubContext, IOptions<BaseUrlOptions> baseUrlOptions)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _hubContext = hubContext;
        _baseUrlOptions = baseUrlOptions.Value;
    }

    public async Task<Result> AcceptRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _context.FriendRequests.FirstOrDefaultAsync(fr => fr.Id == requestId);
        if (request == null) return new NotFoundException(nameof(FriendRequest), requestId);
        
        if (request.ReceiverUserId != userId)
        {
            return new ForbiddenException(userId);
        }

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
            senderUser.Interests.Intersect(receiverUser.Interests).Any())
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

    public async Task<Result> DeclineRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _context.FriendRequests.FirstOrDefaultAsync(fr => fr.Id == requestId);
        if (request == null) return new NotFoundException(nameof(FriendRequest), requestId);

        if (request.ReceiverUserId != userId)
        {
            return new ForbiddenException(userId);
        }

        request.RequestStatus = RequestStatus.Declined;

        await _context.SaveChangesAsync();
        
        return Result.Success();
    }

    public async Task<Result<IEnumerable<FriendRequestDto>>> GetSentRequestAsync(RequestSearchParameters searchParameters, Guid userId)
    {
        var requests = await GetRequests(searchParameters, userId, false)
            .Include(r => r.ReceiverUser)
            .ThenInclude(u => u.Interests)
            .ToListAsync();

        var linkedList = new LinkedList<FriendRequestDto>();

        foreach (var request in requests)
        {
            linkedList.AddLast(new FriendRequestDto
            {
                Id = request.Id,
                User = _mapper.Map<UserShortDto>(request.ReceiverUser),
                RequestStatus = request.RequestStatus
            });
        }

        return linkedList;
    }

    public async Task<Result<IEnumerable<FriendRequestDto>>> GetReceivedRequestAsync(RequestSearchParameters searchParameters, Guid userId)
    {
        var requests = await GetRequests(searchParameters, userId, true)
            .Include(r => r.SenderUser)
            .ThenInclude(u => u.Interests)
            .ToListAsync();

        var linkedList = new LinkedList<FriendRequestDto>();

        foreach (var request in requests)
        {
            linkedList.AddLast(new FriendRequestDto
            {
                Id = request.Id,
                User = _mapper.Map<UserShortDto>(request.SenderUser),
                RequestStatus = request.RequestStatus
            });
        }

        return linkedList;
    }

    public async Task<Result> DeleteRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _context.FriendRequests.FirstOrDefaultAsync(fr => fr.Id == requestId);
        if (request == null) return new NotFoundException(nameof(FriendRequest), requestId);

        if (request.SenderUserId != userId)
        {
            return new ForbiddenException(userId);
        }

        _context.FriendRequests.Remove(request);

        await _context.SaveChangesAsync();
        
        return Result.Success();
    }

    private IQueryable<FriendRequest> GetRequests(RequestSearchParameters searchParameters, Guid userId, bool isReceived)
    {
        var normalizedUserName = searchParameters.UserName?.ToUpper();
        
        var requests = _context.FriendRequests
            .Where(r => (!isReceived && r.SenderUserId == userId) || (isReceived && r.ReceiverUserId == userId))
            .Where(r => normalizedUserName == null || (!isReceived && r.ReceiverUser.NormalizedUserName!.Contains(normalizedUserName)) || (isReceived && r.SenderUser.NormalizedUserName!.Contains(normalizedUserName)))
            .Where(r => searchParameters.RequestStatus == null || r.RequestStatus == searchParameters.RequestStatus)
            .Skip(searchParameters.Offset)
            .Take(searchParameters.Limit);

        return requests;
    }
}