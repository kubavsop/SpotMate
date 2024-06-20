using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.HubModels;
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
        var result = await _userService.GetUserByIdAsync(id, UserId);
        return result.ToIActionResult();
    }
    
    [HttpGet("interest-based-location")]
    public async Task<ActionResult<IEnumerable<UserShortLocationModel>>> GetInterestBaseLocations()
    {
        var result  = await _userService.GetInterestBaseLocations(UserId);
        return result.ToIActionResult();    
    }

    [HttpPost("{id:guid}/request")]
    public async Task<IActionResult> CreateFriendRequest(Guid id)
    {
        var result = await _userService.CreateFriendRequest(UserId, id);
        return result.ToIActionResult();
    }

    [HttpDelete("{id:guid}/request")]
    public async Task<IActionResult> DeleteFriendRequest(Guid id)
    {
        var result = await _userService.DeleteUserRequest(UserId, id);
        return result.ToIActionResult();
    }
    
    [HttpPost("{id:guid}/request/accept")]
    public async Task<IActionResult> AcceptRequest(Guid id)
    {
        var result = await _userService.AcceptRequestAsync(id, UserId);
        return result.ToIActionResult();
    }

    [HttpPost("{id:guid}/request/decline")]
    public async Task<IActionResult> DeclineRequest(Guid id)
    {
        var result = await _userService.DeclineRequestAsync(id, UserId);
        return result.ToIActionResult();    
    }
    
    [HttpPost("{id:guid}/freeze-location")]
    public async Task<IActionResult> FreezeLocation(Guid id)
    {
        var result = await _userService.FreezeLocationAsync(id, UserId);
        return result.ToIActionResult();
    }
    
    [HttpPost("{id:guid}/unfreeze-location")]
    public async Task<IActionResult> UnFreezeLocation(Guid id)
    {
        var result = await _userService.UnFreezeLocationAsync(id, UserId);
        return result.ToIActionResult();    
    }
}