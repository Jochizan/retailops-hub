using Microsoft.AspNetCore.Mvc;
using RetailOps.Application.Interfaces;

namespace RetailOps.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInventory([FromQuery] int storeId)
        {
            if (storeId <= 0)
            {
                return BadRequest("StoreId is required and must be greater than 0.");
            }

            var inventory = await _inventoryService.GetInventoryAsync(storeId);
            return Ok(inventory);
        }
    }
}
