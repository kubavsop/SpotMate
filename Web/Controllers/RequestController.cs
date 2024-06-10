using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Services;
using SpotMate.Web.Controllers.Base;
using SpotMate.Web.Extensions;

namespace SpotMate.Web.Controllers;

[Authorize]
public sealed class RequestController: BaseController
{
    private readonly IRequestService _requestService;

    public RequestController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> AcceptRequest(Guid id)
    {
        var result = await _requestService.AcceptRequestAsync(id, UserId);
        return result.ToIActionResult();
    }

    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> DeclineRequest(Guid id)
    {
        var result = await _requestService.DeclineRequestAsync(id, UserId);
        return result.ToIActionResult();    
    }

    [HttpGet("sent")]
    public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetSentRequest(RequestSearchParameters requestSearchParameters)
    {
        var result = await _requestService.GetSentRequestAsync(requestSearchParameters, UserId);
        return result.ToIActionResult();       
    }
    
    [HttpGet("received")]
    public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetReceivedRequest(RequestSearchParameters requestSearchParameters)
    {
        var result = await _requestService.GetReceivedRequestAsync(requestSearchParameters, UserId);
        return result.ToIActionResult();        
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRequest(Guid id)
    {
        var result = await _requestService.DeleteRequestAsync(id, UserId);
        return result.ToIActionResult();    
    }    
}