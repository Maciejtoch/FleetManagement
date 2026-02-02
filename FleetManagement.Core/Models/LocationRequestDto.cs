using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetManagement.Core.Models
{
    public class LocationRequestDto
    {
        public int VehicleId { get; set; }
        public int Minutes { get; set; }
    }
}
