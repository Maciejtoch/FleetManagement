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

        // 🔹 info o pojeździe usera (read-only w widoku)
        public int VehicleId { get; set; }
        public string? VehicleRegistration { get; set; }

        // 🔹 dane formularza
        public int Mileage { get; set; }
        public string? Notes { get; set; }

        // 🔹 przystanki
        public List<int> SelectedStopIds { get; set; } = new();
        public List<Stop> Stops { get; set; } = new();
    }
}
