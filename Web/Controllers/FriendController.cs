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
public sealed class FriendController: BaseController
{
    private readonly IFriendService _friendService;

    public FriendController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<FriendShortDto>>> GetFriends([FromQuery] UserShortSearchParameters userSearchParameters)
    {
        var result = await _friendService.GetFriendsAsync(userSearchParameters, UserId);
        return result.ToIActionResult();
    }
    
    [HttpGet("location")]
    public async Task<ActionResult<IEnumerable<UserLocationModel>>> GetFriendsLocation()
    {
        var result = await _friendService.GetFriendsLocation(UserId);
        return result.ToIActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FriendDto>> GetFriend(Guid id)
    {
        var result = await _friendService.GetFriend(id, UserId);
        return result.ToIActionResult();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFriend(Guid id)
    {
        var result = await _friendService.DeleteFriendAsync(id, UserId);
        return result.ToIActionResult();
    }
}