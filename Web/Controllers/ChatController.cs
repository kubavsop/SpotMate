using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Base;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Services;
using SpotMate.Web.Controllers.Base;
using SpotMate.Web.Extensions;

namespace SpotMate.Web.Controllers;

[Authorize]
public sealed class ChatController: BaseController
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<ChatDto>>> GetMyChats()
    {
        var result = await _chatService.GetChatsAsync(UserId);
        return result.ToIActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat(CreateChatDto createChatDto)
    {
        var result = await _chatService.CreateChat(createChatDto, UserId);
        return result.ToIActionResult();
    }

    [HttpGet("{id:guid}/message")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(Guid id, [FromQuery] BaseSearchParameters searchParameters)
    {
        var result = await _chatService.GetMessages(id, searchParameters, UserId);
        return result.ToIActionResult();
    }
}