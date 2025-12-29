using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RetailOps.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace RetailOps.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction of the Database Context for the Application layer.
    /// Follows the Interface Segregation Principle to allow services to interact with the DB
    /// without depending on the concrete DbContext implementation.
    /// </summary>
    public interface IRetailOpsDbContext
    {
        DbSet<Store> Stores { get; }
        DbSet<Product> Products { get; }
        DbSet<Sku> Skus { get; }
        DbSet<Inventory> Inventory { get; }
        DbSet<Order> Orders { get; }
        DbSet<OrderItem> OrderItems { get; }
        DbSet<IdempotencyKey> IdempotencyKeys { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<OutboxEvent> OutboxEvents { get; }
        DbSet<StockAlert> StockAlerts { get; }
        DbSet<InventoryMovement> InventoryMovements { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Starts a new database transaction.
        /// </summary>
        /// <remarks>
        /// Although Clean Architecture usually abstracts transactions via UnitOfWork,
        /// we expose this directly here for pragmatism in complex services like OrderService
        /// that require explicit atomic control over multiple aggregates.
        /// </remarks>
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
