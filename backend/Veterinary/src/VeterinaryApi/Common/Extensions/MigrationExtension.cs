using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Common.Extensions;

public static class MigrationExtension
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
        
        
    }
    
}
