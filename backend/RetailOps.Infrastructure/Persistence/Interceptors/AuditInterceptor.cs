using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RetailOps.Domain.Entities;
using System.Text.Json;

namespace RetailOps.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = OnBeforeSaveChanges(context);
        if (auditEntries.Any())
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> OnBeforeSaveChanges(DbContext context)
    {
        context.ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.Entity is OutboxEvent || entry.Entity is IdempotencyKey || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                Timestamp = DateTime.UtcNow,
                UserId = "System", // Default for now, should come from a CurrentUser service
                Action = entry.State.ToString()
            };

            var changes = new Dictionary<string, object>();

            if (entry.State == EntityState.Added)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.IsPrimaryKey()) continue;
                    changes[property.Metadata.Name] = new { NewValue = property.CurrentValue };
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                foreach (var property in entry.Properties)
                {
                    changes[property.Metadata.Name] = new { OldValue = property.OriginalValue };
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.IsModified)
                    {
                        // Skip RowVersion as it's purely for concurrency
                        if (property.Metadata.Name == "RowVersion") continue;

                        changes[property.Metadata.Name] = new
                        {
                            OldValue = property.OriginalValue,
                            NewValue = property.CurrentValue
                        };
                    }
                }
            }

            auditEntry.ChangesJson = JsonSerializer.Serialize(changes);
            
            // For Added entities, Id might not be available yet until after SaveChanges. 
            // We'll leave it empty or update it in a post-save interceptor if critical.
            // For now, capture PK if available.
            var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (key != null && entry.State != EntityState.Added)
            {
                auditEntry.EntityId = key.CurrentValue?.ToString() ?? string.Empty;
            }

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }
}
