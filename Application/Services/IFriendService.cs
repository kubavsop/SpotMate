using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services;

public interface IFriendService
{
    Task<Result<IEnumerable<UserShortDto>>> GetFriendsAsync(UserShortSearchParameters userShortSearchParameters,
        Guid userId);

    Task<Result> DeleteFriendAsync(Guid friendId, Guid userId);
    Task<Result<FriendDto>> GetFriend(Guid friendId, Guid userId);
    Task<Result> FreezeLocationAsync(Guid friendId, Guid userId);
    Task<Result> UnFreezeLocationAsync(Guid friendId, Guid userId);
}