using System;

namespace RetailOps.Domain.Entities
{
    public class Inventory
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int SkuId { get; set; }
        public int OnHand { get; set; }
        public int Reserved { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public Store? Store { get; set; }
        public Sku? Sku { get; set; }
    }
}
