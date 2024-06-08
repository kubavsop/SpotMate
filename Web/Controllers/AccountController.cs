using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Services;
using SpotMate.Infrastructure.Options;
using SpotMate.Web.Controllers.Base;
using SpotMate.Web.Extensions;

namespace SpotMate.Web.Controllers;

public sealed class AccountController: BaseController
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    [Route("register")]
    public async Task<ActionResult<TokenPairDto>> Register(CreateUserDto userDto)
    {
        var result = await _authService.RegisterAsync(userDto);
        return result.ToIActionResult();
    }
    

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<TokenPairDto>> Login(LoginCredentialsDto credentialsDto)
    {
        var result = await _authService.LoginAsync(credentialsDto);
        return result.ToIActionResult();
    }


    [HttpPost]
    [Route("refresh")]
    [Authorize(AuthenticationSchemes = CustomJwtBearerDefaults.CheckOnlySignature)]
    public async Task<ActionResult<TokenPairDto>> Refresh(RefreshDto refreshDto)
    {
        var result = await _authService.RefreshAsync(refreshDto, TokenId);
        return result.ToIActionResult();
    }
    
    [HttpPost]
    [Route("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.LogoutAsync(UserId, TokenId);
        return result.ToIActionResult();
    }
}