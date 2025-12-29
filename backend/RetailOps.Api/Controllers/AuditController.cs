using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOps.Application.Common.Interfaces;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AuditController : ControllerBase
{
    private readonly IRetailOpsDbContext _context;

    public AuditController(IRetailOpsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? entity, [FromQuery] string? action, [FromQuery] int take = 50)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entity))
            query = query.Where(l => l.EntityName == entity);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action == action);

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Take(take)
            .ToListAsync();

        return Ok(logs);
    }
}
