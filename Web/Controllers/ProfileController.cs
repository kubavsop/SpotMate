using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.HubModels;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Exceptions;
using SpotMate.Application.Services;
using SpotMate.Web.Controllers.Base;
using SpotMate.Web.Extensions;

namespace SpotMate.Web.Controllers;

[Authorize]
public sealed class ProfileController: BaseController
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }
    
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var result = await _profileService.GetProfileAsync(UserId);
        return result.ToIActionResult();
    }
    
    [HttpPut]
    public async Task<IActionResult> EditProfile(EditUserDto editUserDto)
    {
        var result = await _profileService.EditProfileAsync(editUserDto, UserId);
        return result.ToIActionResult();
    }

    [HttpPut("user-status")]
    public async Task<IActionResult> EditUserStatus(EditStatusDto editStatusDto)
    {
        var result = await _profileService.EditUserStatus(editStatusDto.UserStatus, UserId);
        return result.ToIActionResult();
    }
    
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(UploadAvatarDto uploadAvatarDto)
    {
        var result = await _profileService.UploadAvatarAsync(uploadAvatarDto, UserId);
        return result.ToIActionResult();
    }
    
    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar()
    {
        var result = await _profileService.DeleteAvatarAsync(UserId);
        return result.ToIActionResult();
    }
    
    [HttpPost("invisible")]
    public async Task<IActionResult> MakeInvisible()
    {
        var result  = await _profileService.MakeInvisibleAsync(UserId);
        return result.ToIActionResult();
    }
    
    [HttpPost("visible")]
    public async Task<IActionResult> MakeVisible()
    {
        var result  = await _profileService.MakeVisibleAsync(UserId);
        return result.ToIActionResult();    
    }

    [HttpPost("share-interest-based-location")]
    public async Task<IActionResult> ShareInterestBasedLocation()
    {
        var result  = await _profileService.ShareInterestBasedLocation(UserId);
        return result.ToIActionResult();    
    }

    [HttpPost("disable-interest-based-location")]
    public async Task<IActionResult> DisableInterestBasedLocation()
    {
        var result  = await _profileService.DisableInterestBasedLocation(UserId);
        return result.ToIActionResult();    
    }
}