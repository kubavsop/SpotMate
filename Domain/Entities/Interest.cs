using SpotMate.Domain.Entities.Base;
using SpotMate.Domain.Enums;

namespace SpotMate.Domain.Entities;

public sealed class Interest: BaseEntity
{
    public required InterestType Type { get; set; }
    public ICollection<SpotMateUser> Users { get; } = [];
}