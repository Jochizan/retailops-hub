using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOps.Application.Common.Interfaces;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly IRetailOpsDbContext _context;

    public StoresController(IRetailOpsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetStores()
    {
        var stores = await _context.Stores
            .AsNoTracking()
            .Select(s => new { s.Id, s.Name, s.Code })
            .ToListAsync();
        return Ok(stores);
    }
}
