using System;
using System.Collections.Generic;

namespace FleetManagement.Core.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Status { get; set; } = "Trial";
        public string PlanType { get; set; } = "Free";
        public bool IsActive { get; set; } = true;
        public DateTime? SubscriptionEndsAt { get; set; }
        public string? SuspensionReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Stop> Stops { get; set; } = new List<Stop>();
        public ICollection<DailyReport> DailyReports { get; set; } = new List<DailyReport>();
        public ICollection<DailyReportStop> DailyReportStops { get; set; } = new List<DailyReportStop>();
        public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
        public ICollection<VehicleLocation> VehicleLocations { get; set; } = new List<VehicleLocation>();
        public ICollection<LocationShareSession> LocationShareSessions { get; set; } = new List<LocationShareSession>();
    }
}
