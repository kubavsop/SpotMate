using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services;

public interface IRequestService
{
    Task<Result> AcceptRequestAsync(Guid requestId, Guid userId);
    Task<Result> DeclineRequestAsync(Guid requestId, Guid userId);
    Task<Result<IEnumerable<FriendRequestDto>>> GetSentRequestAsync(RequestSearchParameters searchParameters,
        Guid userId);
    Task<Result<IEnumerable<FriendRequestDto>>> GetReceivedRequestAsync(RequestSearchParameters searchParameters,
        Guid userId);
    Task<Result> DeleteRequestAsync(Guid requestId, Guid userId);
}