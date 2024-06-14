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

    public bool IsInvisible { get; set; } = false;

    public UserStatus? UserStatus { get; set; }
    public Coordinate? LatestCoordinates { get; set; }

    public ICollection<DailyStep> DailySteps { get; } = [];
    public ICollection<UserFriend> Friends { get; } = [];
    
    public ICollection<RefreshToken> RefreshTokens { get; } = [];
    public ICollection<Interest> Interests { get; } = [];
    public ICollection<FriendRequest> ReceivedRequests { get; } = [];
    
    public ICollection<FriendRequest> SentRequests { get; } = [];
}