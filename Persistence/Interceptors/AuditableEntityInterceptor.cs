using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SpotMate.Domain.Entities.Base;

namespace SpotMate.Persistence.Interceptors;

public sealed class AuditableEntityInterceptor: SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;
        
        var now = DateTime.UtcNow;
        
        foreach (var entity in context.ChangeTracker.Entries<IBaseEntity>())
        {
            switch (entity.State)
            {
                case EntityState.Added:
                    entity.Entity.CreateTime = now;
                    break;
                case EntityState.Modified:
                    entity.Entity.ModifiedTime = now;
                    break;
            }
        }
    }
}