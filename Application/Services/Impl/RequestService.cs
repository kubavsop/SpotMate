using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services.Impl;

public sealed class RequestService: IRequestService
{
    public Task<Result> AcceptRequestAsync(Guid requestId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeclineRequestAsync(Guid requestId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<FriendRequestDto>>> GetSentRequestAsync(RequestSearchParameters searchParameters, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<FriendRequestDto>>> GetReceivedRequestAsync(RequestSearchParameters searchParameters, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteRequestAsync(Guid requestId, Guid userId)
    {
        throw new NotImplementedException();
    }
}