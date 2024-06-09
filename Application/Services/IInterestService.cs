using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services;

public interface IInterestService
{
    Task<Result<IEnumerable<InterestDto>>> GetInterestsAsync();
}