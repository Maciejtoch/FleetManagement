using FleetManagement.Core.Models;
using FleetManagement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        private readonly ITenantContext? _tenantContext;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext? tenantContext = null)
            : base(options)
        {
            _tenantContext = tenantContext;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Stop> Stops { get; set; }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<DailyReportStop> DailyReportStops { get; set; }
        public DbSet<ServiceRecord> ServiceRecords { get; set; }
        public DbSet<VehicleLocation> VehicleLocations { get; set; }
        public DbSet<LocationShareSession> LocationShareSessions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Tenant>()
                .HasIndex(t => t.Slug)
                .IsUnique();

            builder.Entity<Tenant>()
                .HasIndex(t => t.Domain)
                .IsUnique();

            builder.Entity<Tenant>()
                .Property(t => t.Name)
                .IsRequired();

            builder.Entity<Tenant>()
                .Property(t => t.Slug)
                .IsRequired();

            builder.Entity<Tenant>()
                .Property(t => t.Domain)
                .IsRequired();

            builder.Entity<AppUser>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AppUser>()
                .HasOne(u => u.Vehicle)
                .WithOne(v => v.AssignedUser)
                .HasForeignKey<AppUser>(u => u.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Tenant)
                .WithMany(t => t.Vehicles)
                .HasForeignKey(v => v.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Stop>()
                .HasOne(s => s.Tenant)
                .WithMany(t => t.Stops)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DailyReport>()
                .HasOne(r => r.Tenant)
                .WithMany(t => t.DailyReports)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServiceRecord>()
                .HasOne(r => r.Tenant)
                .WithMany(t => t.ServiceRecords)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VehicleLocation>()
                .HasOne(l => l.Tenant)
                .WithMany(t => t.VehicleLocations)
                .HasForeignKey(l => l.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LocationShareSession>()
                .HasOne(s => s.Tenant)
                .WithMany(t => t.LocationShareSessions)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AuditLog>()
                .HasOne(a => a.AdminUser)
                .WithMany()
                .HasForeignKey(a => a.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AuditLog>()
                .HasOne(a => a.Tenant)
                .WithMany()
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<DailyReportStop>()
                .HasKey(x => new { x.DailyReportId, x.StopId });

            builder.Entity<DailyReportStop>()
                .HasOne(x => x.DailyReport)
                .WithMany(r => r.Stops)
                .HasForeignKey(x => x.DailyReportId);

            builder.Entity<DailyReportStop>()
                .HasOne(x => x.Stop)
                .WithMany()
                .HasForeignKey(x => x.StopId);

            builder.Entity<DailyReportStop>()
                .HasOne(x => x.Tenant)
                .WithMany(t => t.DailyReportStops)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            if (_tenantContext != null)
            {
                var isOwner = _tenantContext.IsOwner;
                var tenantId = _tenantContext.TenantId;
                var hasTenantScope = isOwner || tenantId.HasValue;

                if (hasTenantScope)
                {
                    builder.Entity<AppUser>().HasQueryFilter(u => isOwner || !u.TenantId.HasValue || u.TenantId == tenantId);
                    builder.Entity<Vehicle>().HasQueryFilter(v => isOwner || !v.TenantId.HasValue || v.TenantId == tenantId);
                    builder.Entity<Stop>().HasQueryFilter(s => isOwner || !s.TenantId.HasValue || s.TenantId == tenantId);
                    builder.Entity<DailyReport>().HasQueryFilter(r => isOwner || !r.TenantId.HasValue || r.TenantId == tenantId);
                    builder.Entity<ServiceRecord>().HasQueryFilter(r => isOwner || !r.TenantId.HasValue || r.TenantId == tenantId);
                    builder.Entity<VehicleLocation>().HasQueryFilter(l => isOwner || !l.TenantId.HasValue || l.TenantId == tenantId);
                    builder.Entity<LocationShareSession>().HasQueryFilter(s => isOwner || !s.TenantId.HasValue || s.TenantId == tenantId);
                }
            }
        }
    }
}

