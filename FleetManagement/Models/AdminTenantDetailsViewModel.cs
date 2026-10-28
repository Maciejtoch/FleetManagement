using FleetManagement.Core.Models;

namespace FleetManagement.Models
{
    public class AdminTenantDetailsViewModel
    {
        public Tenant Tenant { get; set; } = new();
        public IReadOnlyList<UserListViewModel> Users { get; set; } = new List<UserListViewModel>();
        public IReadOnlyList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public IReadOnlyList<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public int UserCount { get; set; }
        public int VehicleCount { get; set; }
        public DateTime? LastLoginUtc { get; set; }
        public string? LastLoginError { get; set; }
        public string? ActiveTab { get; set; } = "overview";
    }
}
