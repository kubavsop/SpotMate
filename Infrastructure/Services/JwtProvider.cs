using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SpotMate.Application.Services;
using SpotMate.Domain.Entities;
using SpotMate.Infrastructure.Options;

namespace SpotMate.Infrastructure.Services;

public class JwtProvider: IJwtProvider
{
    private readonly JwtOptions _jwtOptions;

    public JwtProvider(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string Generate(SpotMateUser user, Guid tokenId, out DateTime expireAt)
    {
        expireAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireMinutes);
        
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: GetClaims(user, tokenId),
            expires: expireAt,
            signingCredentials: GetSigningCredentials());

        var tokenValue = new JwtSecurityTokenHandler()
            .WriteToken(token);
        
        return tokenValue;
    }


    private SigningCredentials GetSigningCredentials()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    private IEnumerable<Claim> GetClaims(SpotMateUser user, Guid tokenId) =>
    [
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString())
    ];
}