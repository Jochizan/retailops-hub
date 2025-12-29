namespace RetailOps.Domain.Entities;

public class StockAlert
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int SkuId { get; set; }
    public string Type { get; set; } = "stock_low"; // e.g., stock_low, stock_out
    public string Status { get; set; } = "open"; // open, acknowledged, resolved
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Store? Store { get; set; }
    public Sku? Sku { get; set; }
}
