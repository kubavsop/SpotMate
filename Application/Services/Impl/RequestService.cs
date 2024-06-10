using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Exceptions;
using SpotMate.Application.OperationResult;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.Services.Impl;

public sealed class RequestService: IRequestService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RequestService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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
            FirstUserId = request.SenderUserId,
            SecondUserId = request.ReceiverUserId
        });

        _context.FriendRequests.Remove(request);

        await _context.SaveChangesAsync();

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