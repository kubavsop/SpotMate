using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class UserFriend: BaseEntity
{
    public Guid UserId { get; set; }
    public SpotMateUser User  { get; set; } = null!;
    public Guid FriendId  { get; set; }
    public SpotMateUser Friend  { get; set; } = null!;
}