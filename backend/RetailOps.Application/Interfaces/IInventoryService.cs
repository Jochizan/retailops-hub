using System.Collections.Generic;
using System.Threading.Tasks;
using RetailOps.Application.DTOs;

namespace RetailOps.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<List<InventoryDto>> GetInventoryAsync(int storeId);
    }
}
