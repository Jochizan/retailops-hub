using Microsoft.EntityFrameworkCore;
using RetailOps.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace RetailOps.Application.Common.Interfaces
{
    public interface IRetailOpsDbContext
    {
        DbSet<Store> Stores { get; }
        DbSet<Product> Products { get; }
        DbSet<Sku> Skus { get; }
        DbSet<Inventory> Inventory { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
