using RetailOps.Application.DTOs;

namespace RetailOps.Application.Interfaces;

public interface IOrderService
{
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<List<OrderDto>> GetOrdersAsync(int? storeId, string? status);
    Task ConfirmOrderAsync(int id);
    Task CancelOrderAsync(int id);
}
