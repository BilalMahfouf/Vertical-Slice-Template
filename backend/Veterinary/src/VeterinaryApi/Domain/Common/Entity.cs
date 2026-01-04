using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace VeterinaryApi.Domain.Common;

public class Entity : IEntity,ISoftDelete
{
    public Guid Id { get; protected set; }
    public DateTime CreatedOnUtc { get;  set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }

    public void Delete()
    {
        IsDeleted = true;
        DeletedOnUtc = DateTime.UtcNow;
    }
    public Entity()
    {
        Id = Guid.NewGuid();
        CreatedOnUtc = DateTime.UtcNow;
        IsDeleted = false;
    }
}
public interface IEntity
{
    public Guid Id { get; }
    public DateTime CreatedOnUtc { get; }
}
