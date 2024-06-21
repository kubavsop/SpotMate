using AutoMapper;
using Microsoft.AspNetCore.Identity;
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

public class ProfileService: IProfileService
{
    private readonly IDistributedCache _cache;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<SpotMateUser> _userManager;
    private readonly IFileProvider _fileProvider;
    private readonly IHubContext<LocationHub, ILocationHub> _hubContext;
    private readonly BaseUrlOptions _baseUrlOptions;


    public ProfileService(IApplicationDbContext context, IMapper mapper, UserManager<SpotMateUser> userManager, IFileProvider fileProvider, IDistributedCache cache, IHubContext<LocationHub, ILocationHub> hubContext, IOptions<BaseUrlOptions> baseUrlOptions)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
        _fileProvider = fileProvider;
        _cache = cache;
        _hubContext = hubContext;
        _baseUrlOptions = baseUrlOptions.Value;
    }

    public async Task<Result<UserDto>> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return new NotFoundException(nameof(SpotMateUser), userId);
        }
        
        return _mapper.Map<UserDto>(user);
    }

    public async Task<Result> EditProfileAsync(EditUserDto dto, Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            return new NotFoundException(nameof(SpotMateUser), userId);
        }
        
        user.FullName = dto.FullName;
        user.Birthday = dto.Birthday;
        user.Gender = dto.Gender;
        
        var interestsResult = await ChangeInterests(user, dto.Interests.Distinct().ToList());
        if (interestsResult.IsFailure) return interestsResult.Exception;
        
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UploadAvatarAsync(UploadAvatarDto uploadAvatarDto, Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new NotFoundException(nameof(SpotMateUser), userId);
        }

        if (user.AvatarFileName != null)
        {
            _fileProvider.DeleteStaticFileAsync(user.AvatarFileName);
        }
        
        byte[] file;
        using (var stream = new MemoryStream())
        {
            await uploadAvatarDto.Avatar.CopyToAsync(stream);
            file = stream.ToArray();   
        }

        var fileName = Guid.NewGuid() + Path.GetExtension(uploadAvatarDto.Avatar.FileName);
        await _fileProvider.PutStaticFileAsync(file, fileName);
        
        user.AvatarFileName = fileName;
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAvatarAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new NotFoundException(nameof(SpotMateUser), userId);
        }

        if (user.AvatarFileName == null)
        {
            return new BadRequestException("The user does not have an avatar");
        }
        
        _fileProvider.DeleteStaticFileAsync(user.AvatarFileName);
        
        user.AvatarFileName = null;
        await _context.SaveChangesAsync();
        return Result.Success();
    }
    
    public Task<Result> MakeVisibleAsync(Guid userId)
    {
        return ChangeVisibility(userId, false);
    }

    public Task<Result> MakeInvisibleAsync(Guid userId)
    {
        return ChangeVisibility(userId, true);
    }

    public async Task<Result> EditUserStatus(UserStatus? userStatus, Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new NotFoundException(nameof(SpotMateUser), userId);
        }

        user.UserStatus = userStatus;

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ShareInterestBasedLocation(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return new NotFoundException(nameof(SpotMateUser), userId);

        if (user.IsInterestBasedLocationSharable)
            return new BadRequestException("Location is already Sharable");

        user.IsInterestBasedLocationSharable = true;
        await _context.SaveChangesAsync();

        var friends = await _context.UserFriends
            .AsNoTracking()
            .Where(uf => uf.UserId == userId)
            .Select(uf => uf.FriendId)
            .ToListAsync();
        
        var interestsId = user.Interests.Select(i => i.Id).ToList();
        await NotifyUserThatYouHaveInterests([], interestsId, user, friends);
        
        return Result.Success();
    }

    public async Task<Result> DisableInterestBasedLocation(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return new NotFoundException(nameof(SpotMateUser), userId);

        if (!user.IsInterestBasedLocationSharable)
            return new BadRequestException("Location is already disabled");
        
        user.IsInterestBasedLocationSharable = false;
        await _context.SaveChangesAsync();
        
        var friends = await _context.UserFriends
            .AsNoTracking()
            .Where(uf => uf.UserId == userId)
            .Select(uf => uf.FriendId)
            .ToListAsync();
        
        var interestsId = user.Interests.Select(i => i.Id).ToList();
        await NotifyUserThatYouHaveNoInterests([], interestsId, userId, friends);
        
        return Result.Success();
    }

    private async Task<Result> ChangeVisibility(Guid userId, bool isInvisible)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new NotFoundException(nameof(SpotMateUser), userId);
        }

        user.IsInvisible = isInvisible;
        
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }

    private async Task<Result> ChangeInterests(SpotMateUser user, List<Guid> interests)
    {
        var friends = await _context.UserFriends
            .AsNoTracking()
            .Where(uf => uf.UserId == user.Id)
            .Select(uf => uf.FriendId)
            .ToListAsync();
        
        var interestsId = user.Interests.Select(i => i.Id).ToList();
        await NotifyUserThatYouHaveInterests(interestsId.Intersect(interests), interests.Except(interestsId), user, friends);
        await NotifyUserThatYouHaveNoInterests(interests, interestsId.Except(interests), user.Id, friends);
        
        var dictionaryOfUserInterests = user.Interests.ToDictionary(i => i.Id);
        var dictionaryOfInterests = await _context.Interests.ToDictionaryAsync(i => i.Id);

        foreach (var interest in interests)
        {
            if (dictionaryOfUserInterests.ContainsKey(interest)) continue;

            if (!dictionaryOfInterests.ContainsKey(interest))
            {
                return new NotFoundException(nameof(Interest), interest);
            }

            await _context.UserInterests.AddAsync(new UserInterest
            {
                UserId = user.Id,
                InterestId = interest
            });
        }

        foreach (var deleteId in dictionaryOfUserInterests.Keys.Except(interests))
        {
            _context.UserInterests.Remove(
                (await _context.UserInterests.FirstOrDefaultAsync(ui =>
                    ui.UserId == user.Id && ui.InterestId == deleteId))!);
        }

        return Result.Success();
    }

    private async Task NotifyUserThatYouHaveInterests(IEnumerable<Guid> interestsToExcept, IEnumerable<Guid> interestsToNotify, SpotMateUser spotMateUser, IEnumerable<Guid> friends)
    {
        
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id != spotMateUser.Id && !friends.Contains(u.Id) && u.IsInterestBasedLocationSharable && u.Interests.Select(i => i.Id).Intersect(interestsToNotify).Any() && !u.Interests.Select(i => i.Id).Intersect(interestsToExcept).Any())
            .ToListAsync();

        var defaultCoordinate = new CoordinatesModel
            { Latitude = spotMateUser.Latitude, Longitude = spotMateUser.Longitude };
        
        var userLocationModel = new UserShortLocationModel
        {
            Id = spotMateUser.Id,
            Avatar = spotMateUser.AvatarFileName != null ? $"{_baseUrlOptions.Url}{spotMateUser.AvatarFileName}" : null,
            Coordinate = defaultCoordinate,
            FullName = spotMateUser.FullName,
            LastOnline = spotMateUser.LastOnline,
            UserName = spotMateUser.UserName,
            UserStatus = spotMateUser.UserStatus
        };
        
        var userConnectionId = await _cache.GetStringAsync(spotMateUser.Id.ToString());
        
        foreach (var user in users)
        {
            var connectionId = await _cache.GetStringAsync(user.Id.ToString());
            if (connectionId != null)
            {
                var frozenLocation =
                    await _context.FreezeLocations.FirstOrDefaultAsync(f =>
                        f.UserId == spotMateUser.Id && f.FreezerUserId == user.Id);

                if (frozenLocation != null && frozenLocation.IsLocationFrozen)
                {
                    userLocationModel.Coordinate = new CoordinatesModel
                        { Latitude = frozenLocation.Latitude!.Value, Longitude = frozenLocation.Longitude!.Value };
                }

                await _hubContext.Clients.Client(connectionId).ReceiveAddedUserOfSimilarInterests(userLocationModel);

                userLocationModel.Coordinate = defaultCoordinate;
            }

            if (userConnectionId != null)
            {
                var frozenLocation =
                    await _context.FreezeLocations.FirstOrDefaultAsync(f =>
                        f.UserId == user.Id && f.FreezerUserId == spotMateUser.Id);
                
                var userShort = new UserShortLocationModel
                {
                    Id = user.Id,
                    Avatar = user.AvatarFileName != null ? $"{_baseUrlOptions.Url}{user.AvatarFileName}" : null,
                    Coordinate = frozenLocation != null && frozenLocation.IsLocationFrozen ? new CoordinatesModel
                    { Latitude = frozenLocation.Latitude!.Value, Longitude = frozenLocation.Longitude!.Value } : new CoordinatesModel
                        { Latitude = user.Latitude, Longitude = user.Longitude },
                    FullName = user.FullName,
                    LastOnline = user.LastOnline,
                    UserName = user.UserName,
                    UserStatus = user.UserStatus
                };
                
                await _hubContext.Clients.Client(userConnectionId).ReceiveAddedUserOfSimilarInterests(userShort);
            }
        }
    }
    
    private async Task NotifyUserThatYouHaveNoInterests(IEnumerable<Guid> interestsToExcept, IEnumerable<Guid> interestsToNotify, Guid userId, IEnumerable<Guid> friends)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id != userId && !friends.Contains(u.Id) && u.IsInterestBasedLocationSharable && u.Interests.Select(i => i.Id).Intersect(interestsToNotify).Any() && !u.Interests.Select(i => i.Id).Intersect(interestsToExcept).Any())
            .ToListAsync();

        var userConnectionId = await _cache.GetStringAsync(userId.ToString());
        

        foreach (var user in users)
        {
            var connectionId = await _cache.GetStringAsync(user.Id.ToString());
            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId).ReceiveDeletedUserOfSimilarInterestsId(userId);
            }

            if (userConnectionId != null)
            {
                await _hubContext.Clients.Client(userConnectionId).ReceiveDeletedUserOfSimilarInterestsId(user.Id);
            }
        }   
    }
}