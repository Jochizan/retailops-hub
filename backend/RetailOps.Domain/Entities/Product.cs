using System;
using System.Collections.Generic;

namespace RetailOps.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        
        public ICollection<Sku> Skus { get; set; } = new List<Sku>();
    }
}