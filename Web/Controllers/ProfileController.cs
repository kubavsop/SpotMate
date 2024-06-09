using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
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
}