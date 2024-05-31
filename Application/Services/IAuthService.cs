using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services;

public interface IAuthService
{
    Task<Result<TokenPairDto>> RegisterAsync(CreateUserDto dto);   
    Task<Result<TokenPairDto>> LoginAsync(LoginCredentialsDto dto);
    Task<Result<TokenPairDto>> RefreshAsync(RefreshDto dto);
    Task<Result> LogoutAsync(Guid userId, Guid tokenId);
}