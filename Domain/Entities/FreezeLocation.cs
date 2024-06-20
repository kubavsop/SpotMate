using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public class FreezeLocation: BaseEntity
{
    public Guid UserId { get; set; }
    public SpotMateUser User  { get; set; } = null!;
    public Guid FreezerUserId   { get; set; }
    public SpotMateUser FreezerUser  { get; set; } = null!;

    public bool IsLocationFrozen { get; set; } = false;
    
    public double? Latitude { get; set; }
    
    public double? Longitude { get; set; }
}