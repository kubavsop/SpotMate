using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Exceptions;
using SpotMate.Application.OperationResult;
using SpotMate.Application.Options;
using SpotMate.Domain.Entities;

namespace SpotMate.Application.Services.Impl;

public sealed class AuthService: IAuthService
{
    private readonly RefreshTokenOptions _refreshTokenOptions;
    private readonly UserManager<SpotMateUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(IOptions<RefreshTokenOptions> refreshTokenOptions,
        IApplicationDbContext context,
        UserManager<SpotMateUser> userManager,
        IJwtProvider jwtProvider)
    {
        _refreshTokenOptions = refreshTokenOptions.Value;
        _context = context;
        _userManager = userManager;
        _jwtProvider = jwtProvider;
    }


    public async Task<Result<TokenPairDto>> RegisterAsync(CreateUserDto dto)
    {
        var user = new SpotMateUser
        {
            Email = dto.Email,
            FullName = dto.FullName,
            UserName = dto.UserName,
            Birthday = dto.Birthday,
            Gender = dto.Gender,
        };

        if (await _userManager.FindByEmailAsync(user.Email) != null)
        {
            return new BadRequestException("Email already exists");
        }
        
        var result = await _userManager.CreateAsync(user, dto.Password);
        
        if (!result.Succeeded)
        {
            return new IdentityException(result.Errors.ToList());
        }

        var tokenPair = await GetTokenPairs(user);
        
        return tokenPair;
    }

    public async Task<Result<TokenPairDto>> LoginAsync(LoginCredentialsDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return new BadRequestException("Invalid credentials");
        }

        return await GetTokenPairs(user);
    }

    public async Task<Result<TokenPairDto>> RefreshAsync(RefreshDto dto, Guid accessTokenId)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.AccessTokenId == accessTokenId);
        if (refreshToken == null)
        {
            return new NotFoundException(nameof(RefreshToken));
        }

        if (refreshToken.Token != dto.RefreshToken)
        {
            return new BadRequestException("Invalid refresh token");
        }
        
        _context.RefreshTokens.Remove(refreshToken);

        return await GetTokenPairs(refreshToken.User);
    }

    public async Task<Result> LogoutAsync(Guid userId, Guid tokenId)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.AccessTokenId == tokenId);

        if (refreshToken == null)
        {
            return new NotFoundException("Refresh token was not found");
        }

        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }

    private async Task<TokenPairDto> GetTokenPairs(SpotMateUser user)
    {
        var tokenId = Guid.NewGuid();
        var accessToken = _jwtProvider.Generate(user, tokenId, out var accessExpireAt);
        var refreshToken = GenerateRefreshToken();
        var refreshExpireAt = await SetRefreshToken(user, tokenId, refreshToken);
        await _context.SaveChangesAsync();

        return new TokenPairDto
        {
            AccessToken = accessToken,
            AccessTokenExpiredAt = accessExpireAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiredAt = refreshExpireAt
        };
    }
    
    private async Task<DateTime> SetRefreshToken(SpotMateUser user, Guid tokenId, string refreshToken)
    {
        var expireAt = DateTime.UtcNow.AddHours(_refreshTokenOptions.RefreshTokenExpirationHours);
        
        await _context.RefreshTokens.AddAsync(new RefreshToken
        {
            User = user,
            Token = refreshToken,
            AccessTokenId = tokenId,
            RefreshTokenExpirationTime = expireAt
        });

        return expireAt;
    }
    
    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(_refreshTokenOptions.RefreshTokenBytes));
    }
}