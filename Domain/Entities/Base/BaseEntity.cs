namespace SpotMate.Domain.Entities.Base;

public abstract class BaseEntity: IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? ModifiedTime { get; set; }
}