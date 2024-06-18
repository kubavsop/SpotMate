using SpotMate.Application.DTOs.Base;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services;

public interface IChatService
{
    Task<Result<IEnumerable<ChatDto>>> GetChatsAsync(Guid userId);
    Task<Result> CreateChat(CreateChatDto createChatDto, Guid userId);
    Task<Result<IEnumerable<MessageDto>>> GetMessages(Guid chatId, BaseSearchParameters searchParameters, Guid userId);
}