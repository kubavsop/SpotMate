using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class Chat: BaseEntity
{
    public ICollection<SpotMateUser> Users { get; } = [];
    public ICollection<Message> Messages { get; } = [];
}