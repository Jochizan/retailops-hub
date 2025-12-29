namespace RetailOps.Application.DTOs;

public class StockAlertDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int SkuId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
