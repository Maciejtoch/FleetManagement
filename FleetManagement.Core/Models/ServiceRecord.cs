using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetManagement.Core.Models
{
    public class ServiceRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        
        public Vehicle? Vehicle { get; set; }
        public DateTime ServiceDate { get; set; }
        public int Mileage { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal? Cost { get; set; }

    }
}
