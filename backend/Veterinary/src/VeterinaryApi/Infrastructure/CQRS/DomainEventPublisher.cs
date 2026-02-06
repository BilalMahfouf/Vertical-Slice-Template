using System.Collections.Concurrent;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Infrastructure.CQRS;

public class DomainEventPublisher : IDomainEventPublisher
{
     private readonly IServiceProvider _serviceProvider;
    
    // Cache: EventType -> typeof(IDomainEventHandler<EventType>)
    private static readonly ConcurrentDictionary<Type, Type> _handlerTypeCache = new();

    public DomainEventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync(
        IDomainEvent domainEvent,
        CancellationToken ct = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = _handlerTypeCache.GetOrAdd(
            eventType, 
            t => typeof(IDomainEventHandler<>).MakeGenericType(t));

        // Resolve ALL handlers for this event type
        var handlers = _serviceProvider.GetServices(handlerType);
        
        if (!handlers.Any()) return;

        // Execute handlers in parallel
        var tasks = handlers.Select(handler => 
            InvokeHandlerAsync(handler!, domainEvent, ct));
        
        await Task.WhenAll(tasks);
    }

    private static async Task InvokeHandlerAsync(object handler, IDomainEvent domainEvent, CancellationToken ct)
    {
        // Use dynamic to avoid heavy reflection (compiles to call site caching)
        await ((dynamic)handler).Handle((dynamic)domainEvent, ct);
    }
}
