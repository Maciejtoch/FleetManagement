using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetManagement.Core.Models
{
    public class LocationShareSession
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public bool IsActive => DateTime.UtcNow <= ExpiresAt;
    }

}
