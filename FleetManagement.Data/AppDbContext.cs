using FleetManagement.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<Stop> Stops { get; set; }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<DailyReportStop> DailyReportStops { get; set; }
        public DbSet<ServiceRecord> ServiceRecords { get; set; }
        public DbSet<VehicleLocation> VehicleLocations { get; set; }
        public DbSet<LocationShareSession> LocationShareSessions { get; set; }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasOne(u => u.Vehicle)
                .WithOne(v => v.AssignedUser)
                .HasForeignKey<AppUser>(u => u.VehicleId)
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
        }


    }

}

