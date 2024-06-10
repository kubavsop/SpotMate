using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services.Impl;

public sealed class FriendService: IFriendService
{
    public Task<Result<IEnumerable<UserShortDto>>> GetFriendsAsync(UserShortSearchParameters userShortSearchParameters, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteFriendAsync(Guid friendId, Guid userId)
    {
        throw new NotImplementedException();
    }
}