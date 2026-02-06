using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Common;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Infrastructure.OutboxMessages;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IDomainEventPublisher _publisher;

    public ProcessOutboxMessagesJob(
        ApplicationDbContext context,
        IDomainEventDispatcher domainEventDispatcher,
        IDomainEventPublisher publisher)
    {
        _dbContext = context;
        _domainEventDispatcher = domainEventDispatcher;
        _publisher = publisher;
    }
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public async Task Execute(IJobExecutionContext context)
    {
        var outboxMessages = await _dbContext.Set<OutboxMessage>()
            .OrderBy(e => e.Id)
            .Where(e => e.ProcessedOnUtc == null)
            .Take(20)
            .ToListAsync(context.CancellationToken);
        if (outboxMessages is null || !outboxMessages.Any())
        {
            return;
        }
        List<IDomainEvent> events = new();
        foreach (var outboxMessage in outboxMessages)
        {
            var domainEvent = DeserializeDomainEvent(outboxMessage);
            if (domainEvent is null)
            {
                continue;
            }
            await _publisher.PublishAsync(domainEvent, context.CancellationToken);
            outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
        }
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
    private static IDomainEvent? DeserializeDomainEvent(OutboxMessage outboxMessage)
    {
        var domainEventType = Type.GetType(outboxMessage.Name)!;
        var domainEvent = JsonConvert
            .DeserializeObject(
            outboxMessage.Content, domainEventType, _serializerSettings) as IDomainEvent;
        return domainEvent;
    }
}
