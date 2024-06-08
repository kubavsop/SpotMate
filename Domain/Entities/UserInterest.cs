using Microsoft.AspNetCore.Mvc.Rendering;
using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class UserInterest: BaseEntity
{
    public Guid UserId { get; set; }

    public SpotMateUser User { get; set; } = null!;
    
    public Guid InterestId { get; set; }

    public Interest Interest { get; set; } = null!;
}