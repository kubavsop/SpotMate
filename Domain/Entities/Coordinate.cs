using SpotMate.Domain.Entities.Base;

namespace SpotMate.Domain.Entities;

public sealed class Coordinate: BaseEntity
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public SpotMateUser User { get; set; } = null!;
    public Guid UserId { get; set; }
}