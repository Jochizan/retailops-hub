using System;

namespace RetailOps.Domain.Entities
{
    public class Sku
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string AttributesJson { get; set; } = "{}"; // JSON column
        public decimal Price { get; set; }
        
        public Product? Product { get; set; }
    }
}
