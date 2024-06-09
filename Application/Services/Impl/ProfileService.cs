using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Exceptions;
using SpotMate.Application.OperationResult;
using SpotMate.Application.Options;
using SpotMate.Domain.Entities;

namespace SpotMate.Application.Services.Impl;

public class ProfileService: IProfileService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<SpotMateUser> _userManager;
    private readonly StaticBaseUrlOptions _staticBaseUrl;

    public ProfileService(IApplicationDbContext context, IMapper mapper, UserManager<SpotMateUser> userManager, IOptions<StaticBaseUrlOptions> staticBaseUrl)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
        _staticBaseUrl = staticBaseUrl.Value;
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

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Avatar = userDto.Avatar != null ? _staticBaseUrl + userDto.Avatar : null;
        return userDto;
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
        
        if (await _context.Users.AnyAsync(u => u.UserName == dto.UserName && u.Id != user.Id))
        {
            return new BadRequestException("UserName already exists");
        }
        
        if (dto.Email != user.Email && await _userManager.FindByEmailAsync(dto.Email) != null)
        {
            return new BadRequestException("Email already exists");
        }

        user.Email = dto.Email;
        user.NormalizedEmail = dto.Email.ToUpper();
        user.UserName = dto.UserName;
        user.NormalizedUserName = dto.UserName.ToUpper();
        user.FullName = dto.FullName;
        user.Birthday = dto.Birthday;
        user.Gender = dto.Gender;
        
        var interestsResult = await ChangeInterests(user, dto.Interests.Distinct().ToList());
        if (interestsResult.IsFailure) return interestsResult.Exception;
        
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
}