using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Infrastructure.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentTenant _currentUser;

    public AuditInterceptor(ICurrentTenant currentUser)
    {
        _currentUser = currentUser;
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {

            var addedEntities = eventData.Context.ChangeTracker
               .Entries()
               .Where(e => e.Entity is ICreatedBy && e.State == EntityState.Added)
               .Select(e => e.Entity as ICreatedBy);
            foreach (var entity in addedEntities)
            {
                if (entity is null)
                {
                    continue;
                }
                if (_currentUser.UserId is null)
                {
                    continue;
                }
                entity.CreatedByUserId = _currentUser.UserId.Value;
            }

        }
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
