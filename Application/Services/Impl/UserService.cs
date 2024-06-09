using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

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
}