using System.Collections.Generic;
using System.Threading.Tasks;
using RetailOps.Application.DTOs;

namespace RetailOps.Application.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetProductsAsync(string? search, string? brand, string? category, int page, int pageSize);
    }
}
