using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class UserFriend: BaseEntity
{
    public Guid FirstUserId { get; set; }

    public SpotMateUser FirstUser  { get; set; } = null!;
    
    public Guid SecondUserId  { get; set; }

    public SpotMateUser SecondUser  { get; set; } = null!;
}