using SpotMate.Domain.Entities.Base;
using SpotMate.Domain.Enums;

namespace SpotMate.Domain.Entities;

public sealed class FriendRequest: BaseEntity
{
    public Guid SenderUserId { get; set; }

    public SpotMateUser SenderUser { get; set; } = null!;
    
    public Guid ReceiverUserId { get; set; }

    public SpotMateUser ReceiverUser { get; set; } = null!;
    
    public RequestStatus RequestStatus { get; set; }
}