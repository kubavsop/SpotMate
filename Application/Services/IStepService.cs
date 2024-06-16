using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services;

public interface IStepService
{
    Task<Result> CreateDailyStepAsync(CreateDailyStepDto dto, Guid userId);
    Task<Result<IEnumerable<StepDto>>> GetDailyRatingAsync(Guid userId);
    Task<Result<IEnumerable<StepDto>>> GetWeeklyRatingAsync(Guid userId);
    Task<Result<IEnumerable<StepDto>>> GetMonthlyRatingAsync(Guid userId);
}