using RetailOps.Domain.Enums;

namespace RetailOps.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Store? Store { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
