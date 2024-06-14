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
    private readonly IFileProvider _fileProvider;

    public ProfileService(IApplicationDbContext context, IMapper mapper, UserManager<SpotMateUser> userManager, IFileProvider fileProvider)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
        _fileProvider = fileProvider;
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
        user.UserStatus = dto.UserStatus;
        
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