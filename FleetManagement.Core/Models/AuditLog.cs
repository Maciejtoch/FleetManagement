using System;

namespace FleetManagement.Core.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime DateUtc { get; set; } = DateTime.UtcNow;
        public string AdminUserId { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;

        public AppUser? AdminUser { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
