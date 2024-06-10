using Microsoft.AspNetCore.Identity;
using SpotMate.Domain.Entities.Base;
using SpotMate.Domain.Enums;

namespace SpotMate.Domain.Entities;

public sealed class SpotMateUser: IdentityUser<Guid>, IBaseEntity
{
    public DateTime CreateTime { get; set; }
    public DateTime? ModifiedTime { get; set; }
    public override required string Email { get; set; }
    public override required string UserName { get; set; }
    public string? AvatarFileName { get; set; }
    public string? FullName { get;  set; }
    public DateTime? Birthday { get; set; }
    public Gender? Gender { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; } = [];
    public ICollection<Interest> Interests { get; } = [];
    public ICollection<FriendRequest> ReceivedRequests { get; } = [];
    
    public ICollection<FriendRequest> SentRequests { get; } = [];
}