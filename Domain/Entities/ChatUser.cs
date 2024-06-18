using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class ChatUser: BaseEntity
{
    public SpotMateUser User { get; set; } = null!;
    public Guid UserId { get; set; }
    public SpotMateUser Friend { get; set; } = null!;
    public Guid FriendId { get; set; }
    public Chat Chat = null!;
    public Guid ChatId { get; set; }
    public int UnreadMessagesCount { get; set; } = 0;
}