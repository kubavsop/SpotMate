using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class Message: BaseEntity
{
    public required string Text { get; set; }
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
    public Guid UserId { get; set; }
    public SpotMateUser User { get; set; } = null!;
}