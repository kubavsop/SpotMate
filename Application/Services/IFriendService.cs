using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services;

public interface IFriendService
{
    Task<Result<IEnumerable<UserShortDto>>> GetFriendsAsync(UserShortSearchParameters userShortSearchParameters,
        Guid userId);

    Task<Result> DeleteFriendAsync(Guid friendId, Guid userId);
}