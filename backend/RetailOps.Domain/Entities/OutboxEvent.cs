using System;

namespace RetailOps.Domain.Entities
{
    public class OutboxEvent
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string PayloadJson { get; set; } = string.Empty;
        public DateTime OccurredOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string? Error { get; set; }
    }
}
