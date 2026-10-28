using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManagement.Core.Models
{
    public class DailyReport
    {
        public DailyReport()
        {
            Stops = new List<DailyReportStop>();
        }

        public int Id { get; set; }
        public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public DateTime Date { get; set; }
        public int Mileage { get; set; }
        public string? Notes { get; set; }
        [NotMapped]
        public int? TripDurationMinutes { get; set; }
        public ICollection<DailyReportStop> Stops { get; set; }

        public string GetTripDurationDisplay()
        {
            if (TripDurationMinutes.HasValue && TripDurationMinutes.Value > 0)
            {
                return FormatDuration(TripDurationMinutes.Value);
            }

            if (string.IsNullOrWhiteSpace(Notes))
            {
                return "—";
            }

            var marker = "Trip duration:";
            var index = Notes.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var value = Notes.Substring(index + marker.Length).Trim();
                return string.IsNullOrWhiteSpace(value) ? "—" : value;
            }

            return "—";
        }

        public static string FormatDuration(int totalMinutes)
        {
            if (totalMinutes <= 0)
            {
                return "0 min";
            }

            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;

            if (hours > 0 && minutes > 0)
            {
                return $"{hours} h {minutes} min";
            }

            if (hours > 0)
            {
                return $"{hours} h";
            }

            return $"{minutes} min";
        }
    }
}
