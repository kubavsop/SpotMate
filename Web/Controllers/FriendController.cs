using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Services;
using SpotMate.Web.Controllers.Base;
using SpotMate.Web.Extensions;

namespace SpotMate.Web.Controllers;

[Authorize]
public sealed class FriendController: BaseController
{
    private readonly IFriendService _friendService;

    public FriendController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<UserShortDto>>> GetFriends([FromQuery] UserShortSearchParameters userSearchParameters)
    {
        var result = await _friendService.GetFriendsAsync(userSearchParameters, UserId);
        return result.ToIActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FriendDto>> GetFriend(Guid id)
    {
        var result = await _friendService.GetFriend(id, UserId);
        return result.ToIActionResult();
    }
    
    [HttpPost("{id:guid}/freeze-location")]
    public async Task<IActionResult> FreezeLocation(Guid id)
    {
        var result = await _friendService.FreezeLocationAsync(id, UserId);
        return result.ToIActionResult();
    }
    
    [HttpPost("{id:guid}/unfreeze-location")]
    public async Task<IActionResult> UnFreezeLocation(Guid id)
    {
        var result = await _friendService.UnFreezeLocationAsync(id, UserId);
        return result.ToIActionResult();    
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFriend(Guid id)
    {
        var result = await _friendService.DeleteFriendAsync(id, UserId);
        return result.ToIActionResult();
    }
}