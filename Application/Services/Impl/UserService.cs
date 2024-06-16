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
                HasFriendRequest = u.SentRequests.Any(r => r.ReceiverUserId == userId) || u.ReceivedRequests.Any(r => r.SenderUserId == userId),
                Id = u.Id,
                Avatar = u.AvatarFileName != null ? $"http://89.111.175.47:8080/static/{u.AvatarFileName}" : null,
                FullName = u.FullName,
                UserName = u.UserName,
                UserStatus = u.UserStatus
            })
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

    public async Task<Result<UserFullDto>> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            return new NotFoundException(nameof(SpotMateUser), userId);
        }

        return _mapper.Map<UserFullDto>(user);
    }
}