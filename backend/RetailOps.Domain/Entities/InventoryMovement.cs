namespace RetailOps.Domain.Entities;

public class InventoryMovement
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int SkuId { get; set; }
    public string Type { get; set; } = string.Empty; // reserve, confirm, cancel, adjust
    public int Quantity { get; set; }
    public string? Reference { get; set; } // OrderId or AdjustReason
    public DateTime CreatedAt { get; set; }

    public Store? Store { get; set; }
    public Sku? Sku { get; set; }
}
