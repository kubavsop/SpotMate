using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Services;
using SpotMate.Web.Controllers.Base;
using SpotMate.Web.Extensions;

namespace SpotMate.Web.Controllers;

public sealed class ProfileController: BaseController
{
    private readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var result = await _userService.GetProfileAsync(UserId);
        return result.ToIActionResult();
    }
    
    
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> EditProfile(EditUserDto editUserDto)
    {
        var result = await _userService.EditProfileAsync(editUserDto, UserId);
        return result.ToIActionResult();
    }
}