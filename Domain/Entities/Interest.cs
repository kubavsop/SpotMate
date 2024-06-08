using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class Interest: BaseEntity
{
    public required string Name { get; set; }
    public List<SpotMateUser> Users { get; } = [];
}