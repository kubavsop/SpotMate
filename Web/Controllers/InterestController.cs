using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Services;
using SpotMate.Web.Controllers.Base;
using SpotMate.Web.Extensions;

namespace SpotMate.Web.Controllers;

[Authorize]
public sealed class InterestController: BaseController
{
    private readonly IInterestService _interestService;

    public InterestController(IInterestService interestService)
    {
        _interestService = interestService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InterestDto>>> GetInterests()
    {
        var result = await _interestService.GetInterestsAsync();
        return result.ToIActionResult();
    }
}