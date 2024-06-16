using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services.Impl;

public class StepService: IStepService
{
    public Task<Result> CreateDailyStepAsync(CreateDailyStepDto dto, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<StepDto>>> GetDailyRatingAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<StepDto>>> GetWeeklyRatingAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<StepDto>>> GetMonthlyRatingAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}