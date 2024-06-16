using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class DailyStep: BaseEntity
{
    public int StepCount { get; set; }
    
    public DateOnly Date { get; set; }
    public Guid UserId { get; set; }
    public SpotMateUser User { get; set; } = null!;
}