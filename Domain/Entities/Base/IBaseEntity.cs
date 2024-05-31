namespace SpotMate.Domain.Entities.Base;

public interface IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? ModifiedTime { get; set; }
}