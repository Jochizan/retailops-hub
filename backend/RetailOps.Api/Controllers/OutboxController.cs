using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOps.Application.Common.Interfaces;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class OutboxController : ControllerBase
{
    private readonly IRetailOpsDbContext _context;

    public OutboxController(IRetailOpsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] string? type, [FromQuery] bool onlyPending = false, [FromQuery] int take = 50)
    {
        var query = _context.OutboxEvents.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(e => e.Type == type);

        if (onlyPending)
            query = query.Where(e => e.ProcessedOn == null);

        var events = await query
            .OrderByDescending(e => e.OccurredOn)
            .Take(take)
            .ToListAsync();

        return Ok(events);
    }
}
