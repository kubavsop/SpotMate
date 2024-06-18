using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class Chat: BaseEntity
{
    public ICollection<Message> Messages { get; } = [];
    public ICollection<ChatUser> ChatUsers { get; } = [];
}