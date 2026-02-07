using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Infrastructure.OutboxMessages
{
    public class InsertOutboxMessagesInterceptors : SaveChangesInterceptor
    {


        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
       DbContextEventData eventData,
       InterceptionResult<int> result,
       CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                InsertOutboxMessage(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        private static readonly JsonSerializerSettings _serializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private void InsertOutboxMessage(DbContext context)
        {
            var outboxMessages = context.ChangeTracker
                           .Entries<Entity>()
                           .Select(entry => entry.Entity)
                           .SelectMany(e =>
                           {
                               var domainEvents = e.DomainEvents.ToList();
                               e.ClearDomainEvent();
                               return domainEvents;
                           })
                           .Select(@event => new OutboxMessage
                           {
                               Id = Guid.NewGuid(),
                               Name = @event.GetType().AssemblyQualifiedName!,
                               Content = JsonConvert.SerializeObject(@event,
                               _serializerSettings),
                               CreatedOnUtc = DateTime.UtcNow
                           }).ToList();
            context.Set<OutboxMessage>().AddRange(outboxMessages);
        }
    }
}
