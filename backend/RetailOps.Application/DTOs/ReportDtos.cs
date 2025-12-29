namespace RetailOps.Application.DTOs;

public class CriticalStockDto
{
    public int SkuId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int Available { get; set; }
    public int ReorderPoint { get; set; }
    public string StoreName { get; set; } = string.Empty;
}

public class OrdersSummaryDto
{
    public int TotalOrders { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TodaySales { get; set; }
    public double GrowthPercentage { get; set; }
    public Dictionary<string, int> CountsByStatus { get; set; } = new();
}

public class TopSkuDto
{
    public int SkuId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}
