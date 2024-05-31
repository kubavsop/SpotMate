using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Responses;

public sealed class TokenPairDto
{
    [Required]
    public required string AccessToken { get; set; }
    
    [Required]
    public required DateTime AccessTokenExpiredAt { get; set; }
    
    [Required]
    public required string RefreshToken { get; set; }
    
    [Required]
    public required DateTime RefreshTokenExpiredAt { get; set; }
}