using System;

namespace RetailOps.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ChangesJson { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
