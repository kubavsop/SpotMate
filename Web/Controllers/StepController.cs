using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;
using SpotMate.Application.Services;
using SpotMate.Web.Controllers.Base;
using SpotMate.Web.Extensions;

namespace SpotMate.Web.Controllers;

[Authorize]
public sealed class StepController: BaseController
{
    private readonly IStepService _stepService;

    public StepController(IStepService stepService)
    {
        _stepService = stepService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDailyStep(CreateDailyStepDto createDailyStepDto)
    {
        var result = await _stepService.CreateDailyStepAsync(createDailyStepDto, UserId);
        return result.ToIActionResult();
    }

    [HttpGet("rating/dayly")]
    public async Task<ActionResult<IEnumerable<StepDto>>> GetDailyRating()
    {
        var result = await _stepService.GetDailyRatingAsync(UserId);
        return result.ToIActionResult();
    }
    
    [HttpGet("rating/weekly")]
    public async Task<ActionResult<IEnumerable<StepDto>>> GetWeeklyRating()
    {
        var result = await _stepService.GetWeeklyRatingAsync(UserId);
        return result.ToIActionResult();
    }
    
    [HttpGet("rating/monthly")]
    public async Task<ActionResult<IEnumerable<StepDto>>> GetMonthlyRating()
    {
        var result = await _stepService.GetMonthlyRatingAsync(UserId);
        return result.ToIActionResult();
    }
}