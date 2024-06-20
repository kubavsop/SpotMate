using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services;

public interface IUserService
{
    Task<Result<IEnumerable<NonFriendDto>>> GetNonFriendsUsersAsync(UserSearchParameters searchParameters, Guid userId);
    Task<Result> CreateFriendRequest(Guid senderUserId, Guid receiverUserId);
    Task<Result<UserFullDto>> GetUserByIdAsync(Guid userId, Guid myId);
    Task<Result> DeleteUserRequest(Guid senderUserId, Guid receiverUserId);
    
    Task<Result> AcceptRequestAsync(Guid userId, Guid myId);
    Task<Result> DeclineRequestAsync(Guid userId, Guid myId);
    Task<Result> FreezeLocationAsync(Guid freezerUserId, Guid userId);
    Task<Result> UnFreezeLocationAsync(Guid freezerUserId, Guid userId);
}