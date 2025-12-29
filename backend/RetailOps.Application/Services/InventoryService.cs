using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RetailOps.Application.DTOs;
using RetailOps.Application.Interfaces;
using RetailOps.Application.Common.Interfaces;

namespace RetailOps.Application.Services
{
    /// <summary>
    /// Service responsible for querying inventory levels.
    /// Note: Stock modifications (reservations) are handled by OrderService to ensure atomicity.
    /// This service focuses on read operations.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IRetailOpsDbContext _context;

        public InventoryService(IRetailOpsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves current inventory levels for a specific store.
        /// </summary>
        /// <param name="storeId">The Store ID to filter by.</param>
        /// <returns>List of inventory records including product and SKU details.</returns>
        public async Task<List<InventoryDto>> GetInventoryAsync(int storeId)
        {
            var inventory = await _context.Inventory
                .Include(i => i.Sku)
                .ThenInclude(s => s.Product)
                .Where(i => i.StoreId == storeId)
                .ToListAsync();

            return inventory.Select(i => new InventoryDto
            {
                SkuId = i.SkuId,
                SkuCode = i.Sku?.Code ?? string.Empty,
                ProductName = i.Sku?.Product?.Name ?? string.Empty,
                OnHand = i.OnHand,
                Reserved = i.Reserved
            }).ToList();
        }
    }
}
