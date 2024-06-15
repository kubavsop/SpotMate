using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Services;
using SpotMate.Web.Controllers.Base;
using SpotMate.Web.Extensions;

namespace SpotMate.Web.Controllers;

[Authorize]
public sealed class UserController: BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("non-friends")]
    public async Task<ActionResult<IEnumerable<NonFriendDto>>> GetUsers([FromQuery] UserSearchParameters searchParameters)
    {
        var result = await _userService.GetNonFriendsUsersAsync(searchParameters, UserId);
        return result.ToIActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserFullDto>> GetUser(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result.ToIActionResult();
    }

    [HttpPost("{id:guid}/request")]
    public async Task<IActionResult> CreateFriendRequest(Guid id)
    {
        var result = await _userService.CreateFriendRequest(UserId, id);
        return result.ToIActionResult();
    }

    [HttpPost("invisible")]
    public async Task<IActionResult> MakeInvisible()
    {
        var result  = await _userService.MakeInvisibleAsync(UserId);
        return result.ToIActionResult();
    }
    
    [HttpPost("visible")]
    public async Task<IActionResult> MakeVisible()
    {
        var result  = await _userService.MakeVisibleAsync(UserId);
        return result.ToIActionResult();    
    }
}