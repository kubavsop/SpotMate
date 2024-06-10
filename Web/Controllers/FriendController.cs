using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Web.Controllers.Base;

namespace SpotMate.Web.Controllers;

[Authorize]
public sealed class FriendController: BaseController
{
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<UserShortDto>>> GetFriends(UserShortSearchParameters userSearchParameters)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFriend()
    {
        throw new NotImplementedException();
    }
}