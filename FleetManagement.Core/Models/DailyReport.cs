using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetManagement.Core.Models
{
    public class DailyReport
    {
        public DailyReport()
        {
            Stops = new List<DailyReportStop>();
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public DateTime Date { get; set; }
        public int Mileage { get; set; }
        public string? Notes { get; set; }
        public ICollection<DailyReportStop> Stops { get; set; }
    }
}
