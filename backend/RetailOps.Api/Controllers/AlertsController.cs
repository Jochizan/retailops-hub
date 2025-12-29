using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOps.Application.Common.Interfaces;
using RetailOps.Application.DTOs;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IRetailOpsDbContext _context;

    public AlertsController(IRetailOpsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAlerts([FromQuery] int? storeId, [FromQuery] string? status)
    {
        var query = _context.StockAlerts
            .Include(a => a.Store)
            .Include(a => a.Sku)
            .ThenInclude(s => s!.Product)
            .AsNoTracking()
            .AsQueryable();

        if (storeId.HasValue)
            query = query.Where(a => a.StoreId == storeId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        var alerts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new StockAlertDto
            {
                Id = a.Id,
                StoreId = a.StoreId,
                StoreName = a.Store!.Name,
                SkuId = a.SkuId,
                SkuCode = a.Sku!.Code,
                ProductName = a.Sku.Product!.Name,
                Type = a.Type,
                Status = a.Status,
                Message = a.Message,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(alerts);
    }

    [HttpPost("{id}/ack")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var alert = await _context.StockAlerts.FindAsync(id);
        if (alert == null) return NotFound();

        if (alert.Status == "open")
        {
            alert.Status = "acknowledged";
            alert.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        return Ok();
    }

    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id)
    {
        var alert = await _context.StockAlerts.FindAsync(id);
        if (alert == null) return NotFound();

        alert.Status = "resolved";
        alert.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(CancellationToken.None);

        return Ok();
    }
}
