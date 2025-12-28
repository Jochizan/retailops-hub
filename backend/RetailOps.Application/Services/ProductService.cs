using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RetailOps.Application.DTOs;
using RetailOps.Application.Interfaces;
using RetailOps.Application.Common.Interfaces;

namespace RetailOps.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRetailOpsDbContext _context;

        public ProductService(IRetailOpsDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductDto>> GetProductsAsync(string? search, string? brand, string? category, int page, int pageSize)
        {
            var query = _context.Products.Include(p => p.Skus).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(p => p.Brand == brand);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                Category = p.Category,
                Skus = p.Skus.Select(s => new SkuDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    AttributesJson = s.AttributesJson
                }).ToList()
            }).ToList();
        }
    }
}
