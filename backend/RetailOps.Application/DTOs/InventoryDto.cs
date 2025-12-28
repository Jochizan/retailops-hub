namespace RetailOps.Application.DTOs
{
    public class InventoryDto
    {
        public int SkuId { get; set; }
        public string SkuCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int OnHand { get; set; }
        public int Reserved { get; set; }
        public int Available => OnHand - Reserved;
    }
}
