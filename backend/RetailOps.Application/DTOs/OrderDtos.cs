using System.ComponentModel.DataAnnotations;

namespace RetailOps.Application.DTOs;

public class CreateOrderItemDto
{
    [Required]
    public int SkuId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}

public class CreateOrderRequest
{
    [Required]
    public int StoreId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderResponse
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
