using RetailOps.Domain.Enums;

namespace RetailOps.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int SkuId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}
