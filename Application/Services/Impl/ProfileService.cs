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
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return new NotFoundException(nameof(SpotMateUser), userId);

        if (user.IsInterestBasedLocationSharable)
            return new BadRequestException("Location is already Sharable");

        user.IsInterestBasedLocationSharable = true;
        await _context.SaveChangesAsync();
        throw new NotImplementedException();
    }

    public Task<Result> DisableInterestBasedLocation(Guid userId)
    {
        throw new NotImplementedException();
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

    private async Task NotifyUserThatYouHaveInterests(IEnumerable<Guid> interests, SpotMateUser spotMateUser)
    {
        var users = _context.Users
            .AsNoTracking()
            .Where(u => u.Id != spotMateUser.Id && u.IsInterestBasedLocationSharable && u.Interests.Select(i => i.Id).Intersect(interests).Any());

        var userLocationModel = new UserLocationModel
        {
            Id = spotMateUser.Id,
            Avatar = spotMateUser.AvatarFileName != null ? $"{_baseUrlOptions.Url}{spotMateUser.AvatarFileName}" : null,
            Coordinate = new CoordinatesModel { Latitude = spotMateUser.Latitude, Longitude = spotMateUser.Longitude },
            FullName = spotMateUser.FullName,
            LastOnline = spotMateUser.LastOnline,
            UserName = spotMateUser.UserName,
            UserStatus = spotMateUser.UserStatus
        };
        
        foreach (var user in users)
        {
            var connectionId = await _cache.GetStringAsync(user.Id.ToString());
            if (connectionId != null)
            {
                
            }   
        }
    }
    
    private async Task NotifyUserThatYouHaveNoInterests(IEnumerable<Guid> interests, Guid userId)
    {
        throw new NotImplementedException();
    }
}