using RetailOps.Application.DTOs;

namespace RetailOps.Application.Interfaces;

public interface IReportsService
{
    Task<List<CriticalStockDto>> GetCriticalStockAsync(int? storeId, int limit);
    Task<OrdersSummaryDto> GetOrdersSummaryAsync(int? storeId, DateTime? from, DateTime? to);
    Task<List<TopSkuDto>> GetTopSkusAsync(int? storeId, DateTime? from, DateTime? to, int limit);
}
