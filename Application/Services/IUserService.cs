using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services;

public interface IUserService
{
    Task<Result<IEnumerable<UserShortDto>>> GetUsersAsync(UserSearchParameters searchParameters, Guid userId);
    Task<Result> CreateFriendRequest(Guid senderUserId, Guid receiverUserId);
}