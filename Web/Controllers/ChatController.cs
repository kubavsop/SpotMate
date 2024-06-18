using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Services;
using SpotMate.Web.Controllers.Base;

namespace SpotMate.Web.Controllers;

[Authorize]
public sealed class ChatController: BaseController
{

    [HttpGet("my")]
    public Task<ActionResult<IEnumerable<ChatDto>>> GetMyChats()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public Task<IActionResult> CreateChat(CreateChatDto createChatDto)
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id:guid}/message")]
    public Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id:guid}/message")]
    public Task<IActionResult> CreateMessage(Guid id, CreateMessageDto dto)
    {
        throw new NotImplementedException();
    }
}