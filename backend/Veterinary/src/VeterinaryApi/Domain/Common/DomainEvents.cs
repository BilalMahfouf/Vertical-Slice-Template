namespace VeterinaryApi.Domain.Common;

public interface IDomainEvent;

public sealed record DomainEvent : IDomainEvent
{
    public Guid Id { get; private set; }
    public DomainEvent()
    {
        Id = Guid.NewGuid();
    }
}
