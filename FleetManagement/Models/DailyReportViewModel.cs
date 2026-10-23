using FleetManagement.Core.Models;

namespace FleetManagement.Models
{
    public class DailyReportViewModel
    {

        public DailyReportViewModel()
        {
            SelectedStopIds = new List<int>();
            Stops = new List<Stop>();
        }

        public int VehicleId { get; set; }
        public string? VehicleRegistration { get; set; }
        public int Mileage { get; set; }
        public string? Notes { get; set; }
        public List<int> SelectedStopIds { get; set; } = new();
        public List<Stop> Stops { get; set; } = new();
    }
}
