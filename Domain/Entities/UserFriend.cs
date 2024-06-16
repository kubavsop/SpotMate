using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class UserFriend: BaseEntity
{
    public Guid UserId { get; set; }
    public SpotMateUser User  { get; set; } = null!;
    public Guid FriendId  { get; set; }
    public SpotMateUser Friend  { get; set; } = null!;

    public bool IsLocationFrozen { get; set; } = false;
    
    public double? Latitude { get; set; }
    
    public double? Longitude { get; set; }
}