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

public class UserService: IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UserService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<UserShortDto>>> GetUsersAsync(UserSearchParameters searchParameters, Guid userId)
    {
        var normalizedUserName = searchParameters.UserName?.ToUpper();

        var myInterests = await _context.UserInterests
            .AsNoTracking()
            .Where(ui => ui.UserId == userId)
            .Select(ui => ui.InterestId)
            .ToListAsync();
        
        var users = await _context.Users
            .AsNoTracking()
            .Include(u => u.Interests)
            .Where(u => u.Id != userId)
            .Where(u => normalizedUserName == null || u.NormalizedUserName == normalizedUserName)
            .Where(u => searchParameters.Interests == null || u.Interests.Select(i => i.Id).Intersect(searchParameters.Interests).Any())
            .Where(u => !searchParameters.IsInterestMatch || myInterests.Count == 0 || u.Interests.Select(i => i.Id).Intersect(myInterests).Any())
            .Skip(searchParameters.Offset)
            .Take(searchParameters.Limit)
            .ToListAsync();

        return _mapper.Map<List<UserShortDto>>(users);
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
        
        if (await _context.UserFriends.AnyAsync(uf => (uf.FirstUserId == senderUserId && uf.SecondUserId == receiverUserId) || (uf.FirstUserId == receiverUserId && uf.SecondUserId == senderUserId)))
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
}