using Microsoft.EntityFrameworkCore;
using RetailOps.Application.Common.Interfaces;
using RetailOps.Application.DTOs;
using RetailOps.Application.Interfaces;

namespace RetailOps.Application.Services;

public class ReportsService : IReportsService
{
    private readonly IRetailOpsDbContext _context;

    public ReportsService(IRetailOpsDbContext context)
    {
        _context = context;
    }

    public async Task<List<CriticalStockDto>> GetCriticalStockAsync(int? storeId, int limit)
    {
        var query = _context.Inventory
            .Include(i => i.Store)
            .Include(i => i.Sku)
            .ThenInclude(s => s!.Product)
            .AsNoTracking();

        if (storeId.HasValue)
        {
            query = query.Where(i => i.StoreId == storeId.Value);
        }

        // available = onHand - reserved <= reorderPoint
        var criticalItems = await query
            .Where(i => (i.OnHand - i.Reserved) <= i.ReorderPoint)
            .OrderBy(i => (i.OnHand - i.Reserved))
            .Take(limit)
            .Select(i => new CriticalStockDto
            {
                SkuId = i.SkuId,
                SkuCode = i.Sku!.Code,
                ProductName = i.Sku.Product!.Name,
                Brand = i.Sku.Product.Brand,
                Available = i.OnHand - i.Reserved,
                ReorderPoint = i.ReorderPoint,
                StoreName = i.Store!.Name
            })
            .ToListAsync();

        return criticalItems;
    }

    public async Task<OrdersSummaryDto> GetOrdersSummaryAsync(int? storeId, DateTime? from, DateTime? to)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .AsQueryable();

        if (storeId.HasValue)
        {
            query = query.Where(o => o.StoreId == storeId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= to.Value);
        }

        var orders = await query.ToListAsync();

        var summary = new OrdersSummaryDto
        {
            TotalOrders = orders.Count,
            TotalAmount = orders.Sum(o => o.TotalAmount),
            TotalItems = orders.Sum(o => o.Items.Sum(i => i.Quantity)),
            CountsByStatus = orders
                .GroupBy(o => o.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };

        // Calculate Growth (Today vs Yesterday)
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        var salesToday = await _context.Orders
            .Where(o => o.CreatedAt >= today && (storeId == null || o.StoreId == storeId))
            .SumAsync(o => o.TotalAmount);

        var salesYesterday = await _context.Orders
            .Where(o => o.CreatedAt >= yesterday && o.CreatedAt < today && (storeId == null || o.StoreId == storeId))
            .SumAsync(o => o.TotalAmount);

        summary.TodaySales = salesToday;

        if (salesYesterday > 0)
        {
            summary.GrowthPercentage = (double)((salesToday - salesYesterday) / salesYesterday) * 100;
        }
        else
        {
            summary.GrowthPercentage = salesToday > 0 ? 100 : 0;
        }

        return summary;
    }

    public async Task<List<TopSkuDto>> GetTopSkusAsync(int? storeId, DateTime? from, DateTime? to, int limit)
    {
        var query = _context.OrderItems
            .Include(i => i.Order)
            .Include(i => i.Sku)
            .ThenInclude(s => s!.Product)
            .AsNoTracking()
            .AsQueryable();

        if (storeId.HasValue)
            query = query.Where(i => i.Order!.StoreId == storeId.Value);

        if (from.HasValue)
            query = query.Where(i => i.Order!.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(i => i.Order!.CreatedAt <= to.Value);

        var topSkus = await query
            .GroupBy(i => new { i.SkuId, i.Sku!.Code, ProductName = i.Sku.Product!.Name })
            .Select(g => new TopSkuDto
            {
                SkuId = g.Key.SkuId,
                SkuCode = g.Key.Code,
                ProductName = g.Key.ProductName,
                TotalQuantity = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.SubTotal)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(limit)
            .ToListAsync();

        return topSkus;
    }
}
