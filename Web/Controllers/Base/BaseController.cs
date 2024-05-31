using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace SpotMate.Web.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected Guid UserId
    {
        get
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return User.Identity?.IsAuthenticated == null || value == null
                ? Guid.Empty
                : Guid.Parse(value);
        }
    }
    
    protected Guid TokenId
    {
        get
        {
            var value = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            return User.Identity?.IsAuthenticated == null || value == null
                ? Guid.Empty
                : Guid.Parse(value);
        }
    }
}