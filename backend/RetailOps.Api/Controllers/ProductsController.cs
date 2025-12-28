using Microsoft.AspNetCore.Mvc;
using RetailOps.Application.Interfaces;

namespace RetailOps.Api.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? q,
            [FromQuery] string? brand,
            [FromQuery] string? category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _productService.GetProductsAsync(q, brand, category, page, pageSize);
            return Ok(products);
        }
    }
}
