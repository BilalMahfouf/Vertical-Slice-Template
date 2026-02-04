using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Infrastructure.Persistence;

public static class QueryableExtensions
{
    public static IQueryable<T> ForTenant<T>(this DbSet<T> dbSet, Guid TenantId)
        where T : class, ITenantOwned
    {
        return dbSet.Where(e => e.TenantId == TenantId);
    }
}
