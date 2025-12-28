using System.Collections.Generic;

namespace RetailOps.Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<SkuDto> Skus { get; set; } = new List<SkuDto>();
    }

    public class SkuDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string AttributesJson { get; set; } = string.Empty;
    }
}
