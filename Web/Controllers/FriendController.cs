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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFriend(Guid id)
    {
        var result = await _friendService.DeleteFriendAsync(id, UserId);
        return result.ToIActionResult();
    }
}