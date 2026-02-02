using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Domain.Common;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Infrastructure.OutboxMessages;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public ProcessOutboxMessagesJob(
        ApplicationDbContext context,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _dbContext = context;
        _domainEventDispatcher = domainEventDispatcher;
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
            var domainEventType = Type.GetType(outboxMessage.Name);
            var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                outboxMessage.Content, _serializerSettings);
            if (domainEvent is null)
            {
                continue;
            }
            events.Add(domainEvent);
            outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
        }
        await _domainEventDispatcher.DispatchAsync(events, context.CancellationToken);
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
