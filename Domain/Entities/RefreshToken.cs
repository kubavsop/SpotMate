using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class RefreshToken: BaseEntity
{
    public Guid UserId { get; set; }

    public SpotMateUser User { get; set; } = null!;
    public required string Token { get; set; }
    
    public Guid AccessTokenId { get; set; }
    public DateTime? RefreshTokenExpirationTime { get; set; }
    
    public bool RefreshTokenIsExpired => DateTime.UtcNow > RefreshTokenExpirationTime;
}