using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Responses;

public sealed class TokenPairDto
{
    [Required]
    public required string AccessToken { get; init; }
    
    [Required]
    public required DateTime AccessTokenExpiredAt { get; init; }
    
    [Required]
    public required string RefreshToken { get; init; }
    
    [Required]
    public required DateTime RefreshTokenExpiredAt { get; init; }
}